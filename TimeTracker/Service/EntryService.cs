using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Configuration;
using TimeTracker.Model;

namespace TimeTracker.Service
{
    public interface IEntryService
    {
        public Task<LogEntry> AddLogEntry(LogEntry entry);
        public Task<bool> DeleteLogEntry(string id);
        public Task<LogEntry> UpdateLogEntry(LogEntry entry);
        public Task<Collection<LogEntry>> GetLogEntriesByDate(string dateStr);
        public Task<Collection<LogEntry>> GetLogEntriesByYear(int year);
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
                _logger.LogInitializationFailure();
                throw new IOException("Failed to initialize DB connection");
            }
                            
            entry.Id = Guid.NewGuid().ToString();
            entry.PartitionKey = entry.Id;
            var response = await container!.CreateItemAsync(entry, new PartitionKey(entry.PartitionKey));
            _logger.LogLogEntryCreated(response.Resource.Id);

            await _statisticsService.AddLogEntry(entry);
            return response.Resource;
        }

        public async Task<bool> DeleteLogEntry(string id)
        {
            if (!await Initialize())
            {
                _logger.LogInitializationFailure();
                throw new IOException("Failed to initialize DB connection");
            }

            var response = await container!.ReadItemAsync<LogEntry>(id, new PartitionKey(id));
            _logger.LogReadLogEntry(id);
            if (response.Resource != null)
            {
                await _statisticsService.DeleteLogEntry(response.Resource);

                await container!.DeleteItemAsync<LogEntry>(id, new PartitionKey(id));
                _logger.LogDeletedLogEntry(id);
                return true;
            }

            return false;
        }

        public async Task<LogEntry> UpdateLogEntry(LogEntry entry)
        {
            if (!await Initialize())
            {
                _logger.LogInitializationFailure();
                throw new IOException("Failed to initialize DB connection");
            }

            var response = await container!.ReadItemAsync<LogEntry>(entry.Id, new PartitionKey(entry.Id));
            _logger.LogReadLogEntry(entry.Id);
            if (response.Resource != null)
            {
                await _statisticsService.UpdateLogEntry(response.Resource, entry);
            }

            response = await container!.ReplaceItemAsync(entry, entry.Id, new PartitionKey(entry.PartitionKey));
            _logger.LogLogEntryUpdated(response.Resource.Id);
            return response.Resource;
        }

        public async Task<Collection<LogEntry>> GetLogEntriesByDate(string date)
        {
            if (!await Initialize())
            {
                _logger.LogInitializationFailure();
                throw new IOException("Failed to initialize DB connection");
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

        public async Task<Collection<LogEntry>> GetLogEntriesByYear(int year)
        {
            if (!await Initialize())
            {
                _logger.LogInitializationFailure();
                throw new IOException("Failed to initialize DB connection");
            }            

            var sqlQueryText = $"SELECT* FROM c WHERE c.Year = {year}";
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

    internal static class EntryServiceLoggerExtensions
    {
        
        private static readonly Action<ILogger, string, Exception?> _createdLogEntry;        
        private static readonly Action<ILogger, string, Exception?> _deletedLogEntry;
        private static readonly Action<ILogger, string, Exception?> _readLogEntry;
        private static readonly Action<ILogger, string, Exception?> _updatedLogEntry;

        static EntryServiceLoggerExtensions()
        {
            _createdLogEntry = LoggerMessage.Define<string>(
                logLevel: LogLevel.Information,
                eventId: 14,
                formatString: "Inserted log entry {Id} into database.");
            _deletedLogEntry = LoggerMessage.Define<string>(
                logLevel: LogLevel.Information,
                eventId: 14,
                formatString: "Deleted log entry {Id} from database.");
            _readLogEntry = LoggerMessage.Define<string>(
                logLevel: LogLevel.Information,
                eventId: 15,
                formatString: "Found log entry {Id} in database.");
            _updatedLogEntry = LoggerMessage.Define<string>(
                logLevel: LogLevel.Information,
                eventId: 13,
                formatString: "Updated log entry {Id} in database.");
        }                       

        public static void LogDeletedLogEntry(this ILogger logger, string? id)
        {
            _deletedLogEntry(logger, id ?? "", null);
        }

        public static void LogLogEntryCreated(this ILogger logger, string? id)
        {
            _createdLogEntry(logger, id ?? "", null);
        }

        public static void LogReadLogEntry(this ILogger logger, string? id)
        {
            _readLogEntry(logger, id ?? "", null);
        }
     
        public static void LogLogEntryUpdated(this ILogger logger, string? id)
        {
            _updatedLogEntry(logger, id ?? "", null);
        }
    }
}
