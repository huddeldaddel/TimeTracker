using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;
using TimeTracker.Service;

namespace TimeTracker.Functions.LogEntries
{
    public partial class GetLogEntriesForDateFunction
    {
        private readonly ILogger _logger;
        private readonly IEntryService _entryService;

        public GetLogEntriesForDateFunction(ILoggerFactory loggerFactory, IEntryService entryService)
        {
            _logger = loggerFactory.CreateLogger<GetLogEntriesForDateFunction>();
            _entryService = entryService;
        }

        [Function("GetLogEntriesForDate")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "logEntries/{date}")] HttpRequestData req, string date)
        {
            _logger.GetLogEntriesForDateFunctionExecuting(date);
            
            if(!DateRegEx().Match(date).Success)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            
            var response = req.CreateResponse();
            var result = await _entryService.GetLogEntriesByDate(date);
            await response.WriteAsJsonAsync(result);
            return response;
        }

        [GeneratedRegex("\\d{4}-\\d{2}-\\d{2}", RegexOptions.IgnoreCase, "de-DE")]
        private static partial Regex DateRegEx();
    }

    internal static class GetLogEntriesForDateLoggerExtensions
    {
        private static readonly Action<ILogger, string, Exception?> _getLogEntriesForDateFunctionExecuting;

        static GetLogEntriesForDateLoggerExtensions()
        {
            _getLogEntriesForDateFunctionExecuting = LoggerMessage.Define<string>(
                 logLevel: LogLevel.Debug,
                 eventId: 6,
                 formatString: "GetLogEntriesForDateFunction is processing a HTTP trigger for date {Date}");
        }

        public static void GetLogEntriesForDateFunctionExecuting(this ILogger logger, string date)
        {
            _getLogEntriesForDateFunctionExecuting(logger, date, null);
        }
    }
}
