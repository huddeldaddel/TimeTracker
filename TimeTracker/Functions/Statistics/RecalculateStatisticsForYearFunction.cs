using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using TimeTracker.Service;

namespace TimeTracker.Functions.Statistics
{
    public partial class RecalculateStatisticsForYearFunction
    {
        private static readonly Action<ILogger, string, Exception?> _recalculateStatisticsForYearFunctionExecuting = LoggerMessage
            .Define<string>(logLevel: LogLevel.Debug, eventId: 2, formatString: "RecalculateStatisticsForYearFunction is processing a HTTP trigger for year {Year}");

        private readonly ILogger _logger;
        private readonly IEntryService _entryService;
        private readonly IStatisticsService _statisticsService;

        public RecalculateStatisticsForYearFunction(ILoggerFactory loggerFactory, IEntryService entryService, IStatisticsService statisticsService)
        {
            _logger = loggerFactory.CreateLogger<RecalculateStatisticsForYearFunction>();
            _entryService = entryService;
            _statisticsService = statisticsService;
        }

        [Function("RecalculateStatisticsForYear")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "statistics/{year}")] HttpRequestData req, string year)
        {
            _recalculateStatisticsForYearFunctionExecuting.Invoke(_logger, year, null);

            Regex regex = YearRegExp();
            Match match = regex.Match(year);
            if (!match.Success)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var logEntries = await _entryService.GetLogEntriesByYear(int.Parse(year, NumberFormatInfo.InvariantInfo));
            var result = await _statisticsService.RecalculateForYear(logEntries, year);

            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(result);
            return response;
        }

        [GeneratedRegex("\\d{4}")]
        private static partial Regex YearRegExp();
    }
}
