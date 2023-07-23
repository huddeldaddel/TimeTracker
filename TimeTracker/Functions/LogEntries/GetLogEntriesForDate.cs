using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;
using TimeTracker.Service;

namespace TimeTracker.Functions.LogEntries
{
    public partial class GetLogEntriesForDate
    {
        private readonly ILogger _logger;
        private readonly IEntryService _entryService;

        public GetLogEntriesForDate(ILoggerFactory loggerFactory, IEntryService entryService)
        {
            _logger = loggerFactory.CreateLogger<GetLogEntriesForDate>();
            _entryService = entryService;
        }

        [Function("GetLogEntriesForDate")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "logEntries/{date}")] HttpRequestData req, string date)
        {
            _logger.LogInformation($"GetLogEntriesForDate received a request for {date}");
            
            if(!DateRegEx().Match(date).Success)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            
            var response = req.CreateResponse();
            var result = await _entryService.GetEntriesByDate(date);
            await response.WriteAsJsonAsync(result);
            return response;
        }

        [GeneratedRegex("\\d{4}-\\d{1,2}\\d{1,2}", RegexOptions.IgnoreCase, "de-DE")]
        private static partial Regex DateRegEx();
    }
}
