using System;
using System.Collections.ObjectModel;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace CosmosEmulatorSample_Part2
{
    class Program
    {
        private const string CosmosEndpoint = "https://localhost:8081";
        private const string EmulatorKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const string DatabaseId = "LocalLandmarks";
        private const string NaturalSitesCollection = "NaturalSites";

        private static DocumentClient client;

        static void Main(string[] args)
        {
            #region Set up Document client

            // Create the client connection
            client = new DocumentClient(
                new Uri(CosmosEndpoint),
                EmulatorKey,
                new ConnectionPolicy
                {
                    ConnectionMode = ConnectionMode.Direct,
                    ConnectionProtocol = Protocol.Tcp
                });

            #endregion

            #region Create database, collection and indexing policy

            // Set up database and collection Uris
            var databaseUrl = UriFactory.CreateDatabaseUri(DatabaseId);
            var naturalSiteCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseId, NaturalSitesCollection);

            // Create the database if it doesn't exist
            client.CreateDatabaseIfNotExistsAsync(new Database { Id = DatabaseId }).Wait();

            var naturalSitesCollection = new DocumentCollection { Id = NaturalSitesCollection };

            // Create an indexing policy to make strings have a Ranged index.
            var indexingPolicy = new IndexingPolicy();
            indexingPolicy.IncludedPaths.Add(new IncludedPath
            {
                Path = "/*",
                Indexes = new Collection<Microsoft.Azure.Documents.Index>()
                {
                    new RangeIndex(DataType.String) { Precision = -1 }
                }
            });

            // Assign the index policy to our collection
            naturalSitesCollection.IndexingPolicy = indexingPolicy;

            // And create the collection if it doesn't exist
            client.CreateDocumentCollectionIfNotExistsAsync(databaseUrl, naturalSitesCollection).Wait();

            #endregion

            #region Create sample documents in our collection

            // Let's instantiate a POCO with a local landmark
            var giantsCauseway = new NaturalSite { Name = "Giant's Causeway" };

            // Create the document in our database
            client.CreateDocumentAsync(naturalSiteCollectionUri, giantsCauseway).Wait();

            #endregion

            #region Query collection for exact matches

            // Instantiate with the DocumentClient and database identifier
            var cosmosQueryFacade = new CosmosQueryFacade<NaturalSite>
            {
                DocumentClient = client,
                DatabaseId = DatabaseId,
                CollectionId = NaturalSitesCollection
            };

            // We can look for strings that exactly match a search string
            var sites = cosmosQueryFacade.GetItemsAsync(m => m.Name == "Giant's Causeway").Result;

            foreach (var site in sites)
            {
                Console.WriteLine($"The natural site name is: {site.Name}");
            }

            #endregion

            #region Query collection for matches that start with our search string

            // And we can search for strings that start with a search string,
            // as long as we have strings set up to be Ranged Indexes
            sites = cosmosQueryFacade.GetItemsAsync(m => m.Name.StartsWith("Giant")).Result;

            foreach (var site in sites)
            {
                Console.WriteLine($"The natural site name is: {site.Name}");
            }

            #endregion
        }
    }
}
