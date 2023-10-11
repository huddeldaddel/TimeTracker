using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using TimeTracker.Model;
using TimeTracker.Service;

namespace TimeTracker.Functions.Absences
{
    public partial class GetAbsencesForDateRangeFunction
    {
        private readonly ILogger _logger;
        private readonly IStatisticsService _statisticsService;

        public GetAbsencesForDateRangeFunction(ILoggerFactory loggerFactory, IStatisticsService statisticsService)
        {
            _logger = loggerFactory.CreateLogger<GetAbsencesForDateRangeFunction>();
            _statisticsService = statisticsService;
        }

        [Function("GetAbsencesForDateRangeFunction")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "absences/{from}/{to}")] HttpRequestData req, string from, string to)
        {
            _logger.GetAbsencesForDateRangeFunctionExecuting(from, to);
            
            if (! (GetDateRegEx().Match(from).Success && GetDateRegEx().Match(to).Success))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var fromDateTime = DateTime.ParseExact(from, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var toDateTime = DateTime.ParseExact(to, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            if(DateTime.Compare(fromDateTime, toDateTime) > 0)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var result = new Dictionary<string, Absence>();

            var year = fromDateTime.Year;
            var statistics = await _statisticsService.GetByYear(year.ToString(CultureInfo.InvariantCulture.NumberFormat));
            while (DateTime.Compare(fromDateTime, toDateTime) <= 0)
            {
                if (null != statistics)
                {
                    var dateString = fromDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    if (statistics.Days.TryGetValue(dateString, out WorkingDayAggregation? workingDayAggregation))
                    {
                        result.Add(dateString, new Absence(workingDayAggregation));
                    }
                }

                fromDateTime.AddDays(1);
                if((year != fromDateTime.Year) && (DateTime.Compare(fromDateTime, toDateTime) <= 0))
                {
                    year = fromDateTime.Year;
                    statistics = await _statisticsService.GetByYear(year.ToString(CultureInfo.InvariantCulture.NumberFormat));
                }
            }

            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(result);
            return response;
        }

        [GeneratedRegex("\\d{4}-\\d{2}-\\d{2}")]
        private static partial Regex GetDateRegEx();
    }

    internal static class GetAbsencesForDateRangeLoggerExtensions
    {
        private static readonly Action<ILogger, string, string, Exception?> _getAbsencesForDateRangeFunctionExecuting;

        static GetAbsencesForDateRangeLoggerExtensions()
        {
            _getAbsencesForDateRangeFunctionExecuting = LoggerMessage.Define<string, string>(
                 logLevel: LogLevel.Debug,
                 eventId: 1,
                 formatString: "GetAbsencesForDateRangeFunction is processing a HTTP trigger for date range from {From} to {To}");
        }

        public static void GetAbsencesForDateRangeFunctionExecuting(this ILogger logger, string from, string to)
        {
            _getAbsencesForDateRangeFunctionExecuting(logger, from, to, null);
        }
    }
}
