using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using TimeTracker.Model;
using TimeTracker.Service;

namespace TimeTracker.Functions.Absences
{
    public partial class GetAbsencesForDateRangeFunction
    {
        private readonly ILogger _logger;
        private readonly IAbsenceService _absenceService;

        public GetAbsencesForDateRangeFunction(ILoggerFactory loggerFactory, IAbsenceService absenceService)
        {
            _logger = loggerFactory.CreateLogger<GetAbsencesForDateRangeFunction>();
            _absenceService = absenceService;
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

            var dates = new List<string>();
            while (DateTime.Compare(fromDateTime, toDateTime) <= 0)
            {
                var dateString = fromDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                dates.Add(dateString);
                fromDateTime = fromDateTime.AddDays(1);                
            }

            var result = new Dictionary<string, Absence>();
            foreach(var absence in await _absenceService.GetAbsenceByDates(dates))
            {
                result.Add(absence.Date!, absence);
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
