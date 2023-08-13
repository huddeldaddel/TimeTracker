using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Configuration;
using TimeTracker.Functions.LogEntries;
using TimeTracker.Model;

namespace TimeTracker.Service
{
    public interface IEntryService
    {
        public Task<LogEntry> AddLogEntry(LogEntry entry);
        public Task<bool> DeleteLogEntry(string id);
        public Task<LogEntry> UpdateLogEntry(LogEntry entry);
        public Task<Collection<LogEntry>> GetLogEntriesByDate(string date);
    }

    sealed internal class EntryService : IEntryService, IDisposable
    {        
        private readonly CosmosClient cosmosClient;
        private readonly ILogger _logger;
        private readonly IStatisticsService _statisticsService;
        private Database? database;
        private Container? container;

        public EntryService(ILoggerFactory loggerFactory, IStatisticsService statisticsService)
        {
            _logger = loggerFactory.CreateLogger<EntryService>();
            _statisticsService = statisticsService;
            var connectionString = Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING");
            if (null == connectionString)
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


        public async Task<LogEntry> AddLogEntry(LogEntry entry)
        {
            if (!await Initialize())
            {
                throw new Exception("Failed to initialize DB connection");
            }
                            
            entry.Id = Guid.NewGuid().ToString();
            entry.PartitionKey = entry.Id;
            var response = await container!.CreateItemAsync(entry, new PartitionKey(entry.PartitionKey));
            _logger.LogInformation("Created item in database with id: {Id}. Operation consumed {Price} RUs.", response.Resource.Id, response.RequestCharge);

            await _statisticsService.AddLogEntry(entry);
            return response.Resource;
        }

        public async Task<bool> DeleteLogEntry(string id)
        {
            if (!await Initialize())
            {
                throw new Exception("Failed to initialize DB connection");
            }

            var response = await container!.ReadItemAsync<LogEntry>(id, new PartitionKey(id));
            _logger.LogInformation("Search item to delete in database with id: {Id}. Operation consumed {Price} RUs.", id, response.RequestCharge);
            if (response.Resource != null)
            {
                await _statisticsService.DeleteLogEntry(response.Resource);

                response = await container!.DeleteItemAsync<LogEntry>(id, new PartitionKey(id));
                _logger.LogInformation("Deleted item in database with id: {Id}. Operation consumed {Price} RUs.", id, response.RequestCharge);
                return true;
            }

            return false;
        }

        public async Task<LogEntry> UpdateLogEntry(LogEntry entry)
        {
            if (!await Initialize())
            {
                throw new Exception("Failed to initialize DB connection");
            }

            var response = await container!.ReadItemAsync<LogEntry>(entry.Id, new PartitionKey(entry.Id));
            _logger.LogInformation("Search item to update in database with id: {Id}. Operation consumed {Price} RUs.", entry.Id, response.RequestCharge);
            if (response.Resource != null)
            {
                await _statisticsService.UpdateLogEntry(response.Resource, entry);
            }

            response = await container!.ReplaceItemAsync(entry, entry.Id, new PartitionKey(entry.PartitionKey));
            _logger.LogInformation("Updated item in database with id: {Id}. Operation consumed {Price} RUs.", response.Resource.Id, response.RequestCharge);
            return response.Resource;
        }

        public async Task<Collection<LogEntry>> GetLogEntriesByDate(string date)
        {
            if (!await Initialize())
            {
                throw new Exception("Failed to initialize DB connection");
            }

            if(!UpsertLogEntryRequest.ValidateDate(date))
            {
                throw new ArgumentException("Date should be specified as yyyy-MM-dd");
            }

            var sqlQueryText = $"SELECT* FROM c WHERE c.Date = '{date}'";            
            var queryResultSetIterator = container!.GetItemQueryIterator<LogEntry>(new QueryDefinition(sqlQueryText));
            var result = new Collection<LogEntry>();
            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();                
                foreach (LogEntry entry in currentResultSet)
                {
                    result.Add(entry);
                }
            }
            return result;
        }

        public void Dispose()
        {
            this.cosmosClient.Dispose();
        }
    }
}
