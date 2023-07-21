using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TimeTracker.Model;
using TimeTracker.Service;

namespace TimeTracker
{
    public class UpdateLogEntry
    {
        private readonly ILogger _logger;
        private readonly IEntryService _entryService;

        public UpdateLogEntry(ILoggerFactory loggerFactory, IEntryService entryService)
        {
            _logger = loggerFactory.CreateLogger<AddLogEntry>();
            _entryService = entryService;
        }

        [Function("UpdateLogEntry")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "put", Route = "logEntries")] HttpRequestData req)
        {            
            var requestBody = String.Empty;
            using (StreamReader streamReader = new(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            var requestEntry = JsonConvert.DeserializeObject<UpsertEntryRequest>(requestBody);
            _logger.LogInformation("UpdateLogEntry received a request: {body}", requestBody);

            if(null != requestEntry && requestEntry.Validate())
            {
                var result = await _entryService.UpdateEntry(requestEntry.ToEntry()!);
                var response = req.CreateResponse();
                await response.WriteAsJsonAsync(result);
                return response;
            } 
            else
            {                                
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}
