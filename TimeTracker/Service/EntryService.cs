using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Configuration;
using TimeTracker.Model;

namespace TimeTracker.Service
{
    public interface IEntryService
    {
        public Task<Entry> AddEntry(Entry entry);
        public Task<Collection<Entry>> GetEntriesByDate(string date);
    }

    internal class EntryService : IEntryService
    {        
        private readonly CosmosClient cosmosClient;
        private readonly ILogger _logger;
        private Database? database;
        private Container? container;

        public EntryService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AddLogEntry>();
            var connectionString = Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING");
            if(null == connectionString)
            {
                _logger.LogCritical("COSMOS_CONNECTION_STRING not specified!");
                throw new ConfigurationErrorsException("COSMOS_CONNECTION_STRING not specified!");
            }
            cosmosClient = new CosmosClient(connectionString);            
        }

        private async Task<bool> Initialize()
        {
            if (null == database)
            {
                database = await cosmosClient.CreateDatabaseIfNotExistsAsync("TimeTracker");
            }
            if(null == container)
            {
                container = await database.CreateContainerIfNotExistsAsync("LogEntry", "/id");
            }
            return true;
        }


        public async Task<Entry> AddEntry(Entry entry)
        {
            if (!await Initialize())
            {
                throw new Exception("Failed to initialize DB connection");
            }
                            
            entry.Id = Guid.NewGuid().ToString();
            entry.PartitionKey = entry.Id;
            var response = await container!.CreateItemAsync(entry, new PartitionKey(entry.PartitionKey));
            _logger.LogInformation($"Created item in database with id: {response.Resource.Id}. Operation consumed {response.RequestCharge} RUs.");                
            return response.Resource;
        }

        public async Task<Collection<Entry>> GetEntriesByDate(string date)
        {
            if (!await Initialize())
            {
                throw new Exception("Failed to initialize DB connection");
            }

            if(!UpsertEntryRequest.ValidateDate(date))
            {
                throw new ArgumentException("Date should be specified as yyyy-MM-dd");
            }

            var sqlQueryText = $"SELECT* FROM c WHERE c.Date = '{date}'";            
            var queryResultSetIterator = container!.GetItemQueryIterator<Entry>(new QueryDefinition(sqlQueryText));
            var result = new Collection<Entry>();
            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();                
                foreach (Entry entry in currentResultSet)
                {
                    result.Add(entry);
                }
            }
            return result;
        }

    }
}
