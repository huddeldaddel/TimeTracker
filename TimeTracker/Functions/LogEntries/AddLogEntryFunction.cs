using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TimeTracker.Model;
using TimeTracker.Service;

namespace TimeTracker.Functions.LogEntries
{
    public class AddLogEntryFunction
    {
        private static readonly Action<ILogger, string, Exception?> _addEntryFunctionExecuting = LoggerMessage
            .Define<string>(logLevel: LogLevel.Debug, eventId: 4, formatString: "AddLogEntryFunction is processing a HTTP trigger with body {Body}");

        private readonly ILogger _logger;
        private readonly IEntryService _entryService;

        public AddLogEntryFunction(ILoggerFactory loggerFactory, IEntryService entryService)
        {
            _logger = loggerFactory.CreateLogger<AddLogEntryFunction>();
            _entryService = entryService;
        }

        [Function("AddLogEntry")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "logEntries")] HttpRequestData req)
        {
            var requestBody = string.Empty;
            using (StreamReader streamReader = new(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            var requestEntry = JsonConvert.DeserializeObject<UpsertLogEntryRequest>(requestBody);
            _addEntryFunctionExecuting.Invoke(_logger, requestBody, null);

            if (null != requestEntry && requestEntry.Validate())
            {
                var result = await _entryService.AddLogEntry(requestEntry.ToLogEntry()!);
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
