using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using TimeTracker.Service;

namespace TimeTracker.Functions.LogEntries
{
    public class DeleteLogEntryFunction
    {
        private readonly ILogger _logger;
        private readonly IEntryService _entryService;

        public DeleteLogEntryFunction(ILoggerFactory loggerFactory, IEntryService entryService)
        {
            _logger = loggerFactory.CreateLogger<DeleteLogEntryFunction>();
            _entryService = entryService;
        }

        [Function("DeleteLogEntry")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "logEntries/{id}")] HttpRequestData req, string id)
        {
            _logger.DeleteLogEntryFunctionExecuting(id);
            if (null != id && 36 == id.Length)
            {
                var result = await _entryService.DeleteLogEntry(id);
                return req.CreateResponse(result ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.NotFound);
            }
            else
            {
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            }
        }
    }

    internal static class DeleteLogEntryLoggerExtensions
    {
        private static readonly Action<ILogger, string, Exception?> _deleteEntryFunctionExecuting;

        static DeleteLogEntryLoggerExtensions()
        {
            _deleteEntryFunctionExecuting = LoggerMessage.Define<string>(
                logLevel: LogLevel.Debug,
                eventId: 3,
                formatString: "DeleteLogEntryFunction is processing a HTTP trigger for ID {Id}");
        }

        public static void DeleteLogEntryFunctionExecuting(this ILogger logger, string id)
        {
            _deleteEntryFunctionExecuting(logger, id, null);
        }
    }
}
