using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TimeTracker.Model;

namespace TimeTracker
{
    public class AddLogEntry
    {
        private readonly ILogger _logger;

        public AddLogEntry(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AddLogEntry>();
        }

        [Function("AddLogEntry")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "logEntries")] HttpRequestData req)
        {            
            var requestBody = String.Empty;
            using (StreamReader streamReader = new(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            var requestEntry = JsonConvert.DeserializeObject<UpsertEntryRequest>(requestBody);
            _logger.LogInformation($"C# HTTP trigger function processed a request: {requestBody}");

            if(null != requestEntry && requestEntry.Validate())
            {
                return req.CreateResponse(System.Net.HttpStatusCode.OK);
            } 
            else
            {                                
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}
