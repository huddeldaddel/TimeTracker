using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Globalization;
using TimeTracker.Model;

namespace TimeTracker.Service
{
    public interface IAbsenceService
    {        
        public Task<IEnumerable<Absence>> GetAbsenceByDates(IEnumerable<string> dates);
        public Task<Absence> UpdateAbsence(Absence absence);        
    }

    sealed internal class AbsenceService : IAbsenceService, IDisposable
    {
        private readonly CosmosClient cosmosClient;
        private readonly ILogger _logger;        
        private Database? database;
        private Container? container;

        public AbsenceService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AbsenceService>();            
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
                container = await database.CreateContainerIfNotExistsAsync("Absence", "/id");
            }
            return true;
        }

        public async Task<IEnumerable<Absence>> GetAbsenceByDates(IEnumerable<string> dates)
        {
            if (!await Initialize())
            {
                _logger.LogInitializationFailure();
                throw new IOException("Failed to initialize DB connection");
            }

            var keys = new List<(string, PartitionKey)>();                        
            foreach(var date in dates)
            {
                var formattedDate = date.Replace("-", "");
                var key = $"00000000-0000-0000-0000-0000{formattedDate}";
                keys.Add((key, new PartitionKey(key)));                
            }

            var result = new List<Absence>();
            try
            {
                result.AddRange((await container!.ReadManyItemsAsync<Absence>(keys)).Resource);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // This could happen.                
            }

            foreach(var date in dates)
            {
                if(!result.Any((r) => r.Date == date))
                {
                    var dateTime = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    result.Add(new Absence()
                    {
                        Date = date,                        
                        Year = dateTime.Year,
                        Month = dateTime.Month
                    });
                }
            }            

            return result;
        }

        public async Task<Absence> UpdateAbsence(Absence absence)
        {
            if (!await Initialize())
            {
                _logger.LogInitializationFailure();
                throw new IOException("Failed to initialize DB connection");
            }

            var formattedDate = absence.Date?.Replace("-", "");
            var key = $"00000000-0000-0000-0000-0000{formattedDate}";            
            var response = await container!.UpsertItemAsync(absence, new PartitionKey(key));
            _logger.LogLogEntryUpdated(response.Resource.Id);
            return response.Resource;
        }

        public void Dispose()
        {
            this.cosmosClient.Dispose();
        }
    }
}
