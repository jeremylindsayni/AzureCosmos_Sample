using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace CosmosEmulatorSample_Part2
{
    public class CosmosQueryFacade<T> where T : class
    {
        public string CollectionId { get; set; }

        public string DatabaseId { get; set; }

        public DocumentClient DocumentClient { get; set; }

        public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        {
            var documentCollectionUrl = UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId);

            var query = DocumentClient.CreateDocumentQuery<T>(documentCollectionUrl)
                .Where(predicate)
                .AsDocumentQuery();

            var results = new List<T>();

            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }
    }
}