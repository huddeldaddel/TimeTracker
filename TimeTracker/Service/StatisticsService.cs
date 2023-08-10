using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Configuration;
using TimeTracker.Model;

namespace TimeTracker.Service
{
    public interface IStatisticsService
    {
        public Task<LogAggregationByYear> AddLogEntry(LogEntry entry);
        public Task<LogAggregationByYear> DeleteLogEntry(LogEntry entry);
        public Task<LogAggregationByYear?> GetByYear(string year);
        public Task<LogAggregationByYear> UpdateLogEntry(LogEntry oldValue, LogEntry newValue);
    }

    internal class StatisticsService : IStatisticsService
    {
        private readonly CosmosClient cosmosClient;
        private readonly ILogger _logger;
        private Database? database;
        private Container? container;

        public StatisticsService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<StatisticsService>();
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
            if (null == container)
            {
                container = await database.CreateContainerIfNotExistsAsync("Statistics", "/id");
            }
            return true;
        }

        public async Task<LogAggregationByYear> AddLogEntry(LogEntry entry)
        {
            if (!await Initialize())
            {
                throw new Exception("Failed to initialize DB connection");
            }

            var key = $"00000000-0000-0000-0000-00000000{entry.Year}";
            try
            {
                var response = await container!.ReadItemAsync<LogAggregationByYear>(key, new PartitionKey(key));
                _logger.LogInformation("Found existing statistics for {year}. Operation consumed {price} RUs.", entry.Year, response.RequestCharge);

                var updatedLogAggregation = response.Resource;
                updatedLogAggregation.AddLogEntry(entry);
                response = await container!.ReplaceItemAsync(updatedLogAggregation, key, new PartitionKey(key));
                _logger.LogInformation("Updated statistics {year}. Operation consumed {price} RUs.", entry.Year, response.RequestCharge);
                return updatedLogAggregation;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("No statistics found for {year}.", entry.Year);
                var newLogAggregation = new LogAggregationByYear
                {
                    Id = key,
                    PartitionKey = key
                };
                newLogAggregation.AddLogEntry(entry);

                var response = await container!.CreateItemAsync(newLogAggregation, new PartitionKey(key));
                _logger.LogInformation("Inserted new statistics for {year}. Operation consumed {price} RUs.", entry.Year, response.RequestCharge);
                return response.Resource;
            }
        }

        public async Task<LogAggregationByYear> DeleteLogEntry(LogEntry entry)
        {
            if (!await Initialize())
            {
                throw new Exception("Failed to initialize DB connection");
            }

            var key = $"00000000-0000-0000-0000-00000000{entry.Year}";
            try
            {
                var response = await container!.ReadItemAsync<LogAggregationByYear>(key, new PartitionKey(key));
                _logger.LogInformation("Found existing statistics for {year}. Operation consumed {price} RUs.", entry.Year, response.RequestCharge);

                var updatedLogAggregation = response.Resource;
                updatedLogAggregation.RemoveLogEntry(entry);
                response = await container!.ReplaceItemAsync(updatedLogAggregation, key, new PartitionKey(key));
                _logger.LogInformation("Updated statistics {year}. Operation consumed {price} RUs.", entry.Year, response.RequestCharge);
                return updatedLogAggregation;
            } 
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("No statistics found for {year}.", entry.Year);
                return new LogAggregationByYear();
            }                       
        }

        public async Task<LogAggregationByYear?> GetByYear(string year)
        {
            if (!await Initialize())
            {
                throw new Exception("Failed to initialize DB connection");
            }

            var key = $"00000000-0000-0000-0000-00000000{year}";
            try
            {
                var response = await container!.ReadItemAsync<LogAggregationByYear>(key, new PartitionKey(key));
                _logger.LogInformation("Found existing statistics for {year}. Operation consumed {price} RUs.", year, response.RequestCharge);
                return response.Resource;                
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // This could happen. The log entry is not existing.
                _logger.LogWarning("Failed to find statistics for {year}.", year);
                return null;
            }
        }

        public async Task<LogAggregationByYear> UpdateLogEntry(LogEntry oldValue, LogEntry newValue)
        {
            if (!await Initialize())
            {
                throw new Exception("Failed to initialize DB connection");
            }

            if(oldValue.Year != newValue.Year)
            {
                await DeleteLogEntry(oldValue);
                return await AddLogEntry(newValue);
            }

            var key = $"00000000-0000-0000-0000-00000000{newValue.Year}";
            try
            {
                var response = await container!.ReadItemAsync<LogAggregationByYear>(key, new PartitionKey(key));
                _logger.LogInformation("Found existing statistics for {year}. Operation consumed {price} RUs.", newValue.Year, response.RequestCharge);

                var updatedLogAggregation = response.Resource;
                updatedLogAggregation.RemoveLogEntry(oldValue);
                updatedLogAggregation.AddLogEntry(newValue);
                response = await container!.ReplaceItemAsync(updatedLogAggregation, key, new PartitionKey(key));
                _logger.LogInformation("Updated statistics {year}. Operation consumed {price} RUs.", newValue.Year, response.RequestCharge);
                return updatedLogAggregation;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // This should not happen. The log entry is existing and the year didn't change.
                // Means that the statistics are not in sync with log entries :(
                _logger.LogWarning("Failed to find statistics for {year}.", newValue.Year);

                var newLogAggregation = new LogAggregationByYear
                {
                    Id = key,
                    PartitionKey = key
                };

                var response = await container!.CreateItemAsync(newLogAggregation, new PartitionKey(key));
                _logger.LogInformation("Inserted new statistics for {year}. Operation consumed {price} RUs.", newValue.Year, response.RequestCharge);
                return response.Resource;
            }                                                
        }
    }
}
