using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace CosmosEmulatorSample_Part1
{
    class Program
    {
        private static readonly string CosmosEndpoint = "https://localhost:8081";
        private static readonly string EmulatorKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private static readonly string DatabaseId = "LocalLandmarks";
        private static readonly string NaturalSitesCollection = "NaturalSites";

        static void Main(string[] args)
        {
            // Create the client connection
            var client = new DocumentClient(
                new Uri(CosmosEndpoint),
                EmulatorKey,
                new ConnectionPolicy
                {
                    ConnectionMode = ConnectionMode.Direct,
                    ConnectionProtocol = Protocol.Tcp
                });

            // Create a new database in Cosmos
            var databaseCreationResult = client.CreateDatabaseAsync(new Database { Id = DatabaseId }).Result;
            Console.WriteLine("The database Id created is: " + databaseCreationResult.Resource.Id);

            // Now initialize a new collection for our objects to live inside
            var collectionCreationResult = client.CreateDocumentCollectionAsync(
                UriFactory.CreateDatabaseUri(DatabaseId),
                new DocumentCollection { Id = NaturalSitesCollection }).Result;

            Console.WriteLine("The collection created has the ID: " + collectionCreationResult.Resource.Id);

            // Let's instantiate a POCO with a local landmark
            var giantsCauseway = new NaturalSite { Name = "Giant's Causeway" };

            // Add this POCO as a document in Cosmos to our natural site collection
            var itemResult = client
                .CreateDocumentAsync(
                    UriFactory.CreateDocumentCollectionUri(DatabaseId, NaturalSitesCollection), giantsCauseway)
                .Result;

            Console.WriteLine("The document has been created with the ID: " + itemResult.Resource.Id);

            // Use the ID to retrieve the object we just created
            var document = client
                .ReadDocumentAsync(
                    UriFactory.CreateDocumentUri(DatabaseId, NaturalSitesCollection, itemResult.Resource.Id))
                .Result;

            // Convert the document resource returned to a NaturalSite POCO
            NaturalSite site = (dynamic)document.Resource;

            Console.WriteLine("The returned document is a natural landmark with name: " + site.Name);
        }
    }
}
