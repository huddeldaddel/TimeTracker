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
        public Task<Collection<LogEntry>> FindLogEntriesByYearAndProject(int year, string? project, string? query);
    }

    sealed internal class EntryService : IEntryService, IDisposable
    {
        private static readonly Action<ILogger, string, Exception?> _createdLogEntry = LoggerMessage
            .Define<string>(logLevel: LogLevel.Information, eventId: 14, formatString: "Inserted log entry {Id} into database.");
        private static readonly Action<ILogger, string, Exception?> _deletedLogEntry = LoggerMessage
            .Define<string>(logLevel: LogLevel.Information, eventId: 14, formatString: "Deleted log entry {Id} from database.");
        private static readonly Action<ILogger, string, Exception?> _readLogEntry = LoggerMessage
            .Define<string>(logLevel: LogLevel.Information, eventId: 15, formatString: "Found log entry {Id} in database.");
        private static readonly Action<ILogger, string, Exception?> _updatedLogEntry = LoggerMessage
            .Define<string>(logLevel: LogLevel.Information, eventId: 13, formatString: "Updated log entry {Id} in database.");

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
            _createdLogEntry(_logger, response.Resource.Id ?? "", null);

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
            _readLogEntry(_logger, id ?? string.Empty, null);
            if (response.Resource != null)
            {
                await _statisticsService.DeleteLogEntry(response.Resource);                
                await container!.DeleteItemAsync<LogEntry>(id, new PartitionKey(id));
                _deletedLogEntry(_logger, id ?? string.Empty, null);                
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
            _readLogEntry(_logger, entry.Id ?? string.Empty, null);            
            if (response.Resource != null)
            {
                await _statisticsService.UpdateLogEntry(response.Resource, entry);
            }

            response = await container!.ReplaceItemAsync(entry, entry.Id, new PartitionKey(entry.PartitionKey));
            _updatedLogEntry(_logger, response.Resource.Id ?? "", null);            
            return response.Resource;
        }

        public async Task<Collection<LogEntry>> GetLogEntriesByDate(string dateStr)
        {
            if (!await Initialize())
            {
                _logger.LogInitializationFailure();
                throw new IOException("Failed to initialize DB connection");
            }

            if(!UpsertLogEntryRequest.ValidateDate(dateStr))
            {
                throw new ArgumentException("Date should be specified as yyyy-MM-dd");
            }

            var sqlQueryText = $"SELECT* FROM c WHERE c.Date = '{dateStr}'";            
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

        public async Task<Collection<LogEntry>> FindLogEntriesByYearAndProject(int year, string? project, string? query)
        {
            if (!await Initialize())
            {
                _logger.LogInitializationFailure();
                throw new IOException("Failed to initialize DB connection");
            }

            var sqlQueryText = $"SELECT* FROM c WHERE c.Year = @year";
            if(null != project)
            {
                sqlQueryText += $" AND c.Project = @project";
            }
            if(null != query)
            {
                sqlQueryText += $" AND c.Description LIKE @query";
            }

            var queryDefinition = new QueryDefinition(sqlQueryText).WithParameter("@year", year);
            if(null != project)
            {
                queryDefinition = queryDefinition.WithParameter("@project", project);
            }
            if (null != query)
            {
                queryDefinition = queryDefinition.WithParameter("@query", query.Replace("*", "%"));
            }

            var queryResultSetIterator = container!.GetItemQueryIterator<LogEntry>(queryDefinition);
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
