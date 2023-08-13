using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TimeTracker.Model;
using TimeTracker.Service;

namespace TimeTracker.Functions.LogEntries
{
    public class UpdateLogEntryFunction
    {
        private readonly ILogger _logger;
        private readonly IEntryService _entryService;

        public UpdateLogEntryFunction(ILoggerFactory loggerFactory, IEntryService entryService)
        {
            _logger = loggerFactory.CreateLogger<UpdateLogEntryFunction>();
            _entryService = entryService;
        }

        [Function("UpdateLogEntry")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "put", Route = "logEntries")] HttpRequestData req)
        {
            var requestBody = string.Empty;
            using (StreamReader streamReader = new(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            var requestEntry = JsonConvert.DeserializeObject<UpsertLogEntryRequest>(requestBody);
            _logger.LogInformation("UpdateLogEntry received a request: {body}", requestBody);

            if (null != requestEntry && requestEntry.Validate())
            {
                var result = await _entryService.UpdateLogEntry(requestEntry.ToLogEntry()!);
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

    internal static class UpdateLogEntryLoggerExtensions
    {
        private static readonly Action<ILogger, string, Exception?> _updateEntryFunctionExecuting;

        static UpdateLogEntryLoggerExtensions()
        {
            _updateEntryFunctionExecuting = LoggerMessage.Define<string>(
                logLevel: LogLevel.Debug,
                eventId: 5,
                formatString: "UpdateLogEntryFunction is processing a HTTP trigger with body {Body}");
        }

        public static void UpdateLogEntryFunctionExecuting(this ILogger logger, string body)
        {
            _updateEntryFunctionExecuting(logger, body, null);
        }
    }
}
