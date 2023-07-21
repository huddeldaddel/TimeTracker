using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using TimeTracker.Service;

namespace TimeTracker
{
    public class DeleteLogEntry
    {
        private readonly ILogger _logger;
        private readonly IEntryService _entryService;

        public DeleteLogEntry(ILoggerFactory loggerFactory, IEntryService entryService)
        {
            _logger = loggerFactory.CreateLogger<DeleteLogEntry>();
            _entryService = entryService;
        }

        [Function("DeleteLogEntry")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "logEntries/{id}")] HttpRequestData req, string id)
        {                                    
            _logger.LogInformation("DeleteLogEntry received a request: {id}", id);
            if(null != id && 36 == id.Length)
            {
                var result = await _entryService.DeleteEntry(id);
                return req.CreateResponse(result ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.NotFound);                                
            } 
            else
            {                                
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}
