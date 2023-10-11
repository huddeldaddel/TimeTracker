using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using TimeTracker.Model;

namespace TimeTracker.Service
{
    public interface IStatisticsService
    {
        public Task<LogAggregationByYear> AddLogEntry(LogEntry entry);
        public Task<LogAggregationByYear> DeleteLogEntry(LogEntry entry);
        public Task<LogAggregationByYear?> GetByYear(string year);
        public Task<LogAggregationByYear> UpdateLogEntry(LogEntry oldValue, LogEntry newValue);
        public Task<LogAggregationByYear> RecalculateForYear(Collection<LogEntry> logEntries, string year);
    }

    sealed internal class StatisticsService : IStatisticsService, IDisposable
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
                _logger.LogConnectionStringNotSet();
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
                _logger.LogInitializationFailure();
                throw new IOException("Failed to initialize DB connection");
            }

            var key = $"00000000-0000-0000-0000-00000000{entry.Year}";
            try
            {
                var response = await container!.ReadItemAsync<LogAggregationByYear>(key, new PartitionKey(key));
                _logger.LogStatisticsReadForYear(entry.Year);

                var updatedLogAggregation = response.Resource;
                updatedLogAggregation.AddLogEntry(entry);
                response = await container!.ReplaceItemAsync(updatedLogAggregation, key, new PartitionKey(key));
                _logger.LogStatisticsUpdatedForYear(entry.Year);
                return updatedLogAggregation;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogNoStatisticsForYear(entry.Year);
                var newLogAggregation = new LogAggregationByYear
                {
                    Id = key,
                    PartitionKey = key
                };
                newLogAggregation.AddLogEntry(entry);

                var response = await container!.CreateItemAsync(newLogAggregation, new PartitionKey(key));
                _logger.LogStatisticsCreatedForYear(entry.Year);
                return response.Resource;
            }
        }

        public async Task<LogAggregationByYear> DeleteLogEntry(LogEntry entry)
        {
            if (!await Initialize())
            {
                _logger.LogInitializationFailure();
                throw new IOException("Failed to initialize DB connection");
            }

            var key = $"00000000-0000-0000-0000-00000000{entry.Year}";
            try
            {
                var response = await container!.ReadItemAsync<LogAggregationByYear>(key, new PartitionKey(key));
                _logger.LogStatisticsReadForYear(entry.Year);

                var updatedLogAggregation = response.Resource;
                updatedLogAggregation.RemoveLogEntry(entry);
                response = await container!.ReplaceItemAsync(updatedLogAggregation, key, new PartitionKey(key));
                _logger.LogStatisticsUpdatedForYear(entry.Year);
                return updatedLogAggregation;
            } 
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogNoStatisticsForYear(entry.Year);
                return new LogAggregationByYear();
            }                       
        }

        public async Task<LogAggregationByYear?> GetByYear(string year)
        {
            if (!await Initialize())
            {
                _logger.LogInitializationFailure();
                throw new IOException("Failed to initialize DB connection");
            }

            var key = $"00000000-0000-0000-0000-00000000{year}";
            try
            {
                var response = await container!.ReadItemAsync<LogAggregationByYear>(key, new PartitionKey(key));
                _logger.LogStatisticsReadForYear(year);
                return response.Resource;                
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // This could happen. The log entry is not existing.
                _logger.LogNoStatisticsForYear(year);
                return null;
            }
        }

        public async Task<LogAggregationByYear> UpdateLogEntry(LogEntry oldValue, LogEntry newValue)
        {
            if (!await Initialize())
            {
                _logger.LogInitializationFailure();
                throw new IOException("Failed to initialize DB connection");
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
                _logger.LogStatisticsReadForYear(newValue.Year);

                var updatedLogAggregation = response.Resource;
                updatedLogAggregation.RemoveLogEntry(oldValue);
                updatedLogAggregation.AddLogEntry(newValue);
                response = await container!.ReplaceItemAsync(updatedLogAggregation, key, new PartitionKey(key));
                _logger.LogStatisticsUpdatedForYear(newValue.Year);
                return updatedLogAggregation;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // This should not happen. The log entry is existing and the year didn't change.
                // Means that the statistics are not in sync with log entries :(
                _logger.LogNoStatisticsForYear(newValue.Year);

                var newLogAggregation = new LogAggregationByYear
                {
                    Id = key,
                    PartitionKey = key
                };

                var response = await container!.CreateItemAsync(newLogAggregation, new PartitionKey(key));
                _logger.LogStatisticsCreatedForYear(newValue.Year);
                return response.Resource;
            }                                                
        }

        public async Task<LogAggregationByYear> RecalculateForYear(Collection<LogEntry> logEntries, string year)
        {
            if (!await Initialize())
            {
                _logger.LogInitializationFailure();
                throw new IOException("Failed to initialize DB connection");
            }

            var key = $"00000000-0000-0000-0000-00000000{year}";
            try
            {
                await container!.DeleteItemAsync<LogAggregationByYear>(key, new PartitionKey(key));
                _logger.LogStatisticsDeletedForYear(year);                
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // That's OK.
            }

            var newLogAggregation = new LogAggregationByYear
            {
                Id = key,
                PartitionKey = key
            };
            foreach(LogEntry entry in logEntries)
            {
                newLogAggregation.AddLogEntry(entry);
            }

            var response = await container!.CreateItemAsync(newLogAggregation, new PartitionKey(key));
            _logger.LogStatisticsCreatedForYear(int.Parse(year, NumberFormatInfo.InvariantInfo));
            return response.Resource;
        }

        public void Dispose()
        {
            this.cosmosClient?.Dispose();
        }
    }

    internal static class StatisticsServiceLoggerExtensions
    {
        private static readonly Action<ILogger, Exception?> _connectionStringNotSet;
        private static readonly Action<ILogger, string, Exception?> _createdStatisticsForYear;
        private static readonly Action<ILogger, string, Exception?> _deletedStatisticsForYear;
        private static readonly Action<ILogger, Exception?> _initializationFailure;
        private static readonly Action<ILogger, string, Exception?> _noStatisticsForYear;
        private static readonly Action<ILogger, string, Exception?> _readStatisticsForYear;
        private static readonly Action<ILogger, string, Exception?> _updatedStatisticsForYear;

        static StatisticsServiceLoggerExtensions()
        {
            _connectionStringNotSet = LoggerMessage.Define(
                logLevel: LogLevel.Critical,
                eventId: 12,
                formatString: "COSMOS_CONNECTION_STRING not specified!");
            _createdStatisticsForYear = LoggerMessage.Define<string>(
                logLevel: LogLevel.Information,
                eventId: 9,
                formatString: "Inserted new statistics for {Year}.");
            _deletedStatisticsForYear = LoggerMessage.Define<string>(
                logLevel: LogLevel.Information,
                eventId: 13,
                formatString: "Deleted statistics for {Year}.");
            _initializationFailure = LoggerMessage.Define(
                logLevel: LogLevel.Critical,
                eventId: 7,
                formatString: "Failed to initialize DB connection");
            _noStatisticsForYear = LoggerMessage.Define<string>(
                logLevel: LogLevel.Information,
                eventId: 8,
                formatString: "Failed to find statistics for {Year}.");
            _readStatisticsForYear = LoggerMessage.Define<string>(
                logLevel: LogLevel.Information,
                eventId: 10,
                formatString: "Found to statistics for {Year}.");
            _updatedStatisticsForYear = LoggerMessage.Define<string>(
                logLevel: LogLevel.Information,
                eventId: 11,
                formatString: "Updated statistics for {Year}.");
        }

        public static void LogConnectionStringNotSet(this ILogger logger)
        {
            _connectionStringNotSet(logger, null);
        }

        public static void LogInitializationFailure(this ILogger logger)
        {
            _initializationFailure(logger, null);
        }

        public static void LogNoStatisticsForYear(this ILogger logger, string year)
        {
            _noStatisticsForYear(logger, year, null);
        }

        public static void LogNoStatisticsForYear(this ILogger logger, int? year)
        {
            if(null == year)
            {
                _noStatisticsForYear(logger, "", null);
            }
            else
            {
                _noStatisticsForYear(logger, year.ToString()!, null);
            }            
        }

        public static void LogStatisticsCreatedForYear(this ILogger logger, int? year)
        {
            if (null == year)
            {
                _createdStatisticsForYear(logger, "", null);
            }
            else
            {
                _createdStatisticsForYear(logger, year.ToString()!, null);
            }
        }

        public static void LogStatisticsDeletedForYear(this ILogger logger, string year)
        {
            _deletedStatisticsForYear(logger, year, null);
        }

        public static void LogStatisticsReadForYear(this ILogger logger, string year)
        {
            _readStatisticsForYear(logger, year, null);
        }

        public static void LogStatisticsReadForYear(this ILogger logger, int? year)
        {
            if (null == year)
            {
                _readStatisticsForYear(logger, "", null);
            }
            else
            {
                _readStatisticsForYear(logger, year.ToString()!, null);
            }
        }

        public static void LogStatisticsUpdatedForYear(this ILogger logger, int? year)
        {
            if (null == year)
            {
                _updatedStatisticsForYear(logger, "", null);
            }
            else
            {
                _updatedStatisticsForYear(logger, year.ToString()!, null);
            }
        }
    }
}
