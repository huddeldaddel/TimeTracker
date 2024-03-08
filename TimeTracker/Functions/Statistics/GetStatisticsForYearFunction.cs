using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;
using TimeTracker.Service;

namespace TimeTracker.Functions
{
    public partial class GetStatisticsForYearFunction
    {
        private static readonly Action<ILogger, string, Exception?> _getStatisticsForYearFunctionExecuting = LoggerMessage
            .Define<string>(logLevel: LogLevel.Debug, eventId: 2, formatString: "GetStatisticsForYearFunction is processing a HTTP trigger for year {Year}");

        private readonly ILogger _logger;
        private readonly IStatisticsService _statisticsService;

        public GetStatisticsForYearFunction(ILoggerFactory loggerFactory, IStatisticsService statisticsService)
        {
            _logger = loggerFactory.CreateLogger<GetStatisticsForYearFunction>();
            _statisticsService = statisticsService;
        }

        [Function("GetStatisticsForYear")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "statistics/{year}")] HttpRequestData req, string year)
        {
            _getStatisticsForYearFunctionExecuting.Invoke(_logger, year, null);

            Regex rgx = YearRegEx();
            Match match = rgx.Match(year);
            if(!match.Success)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var result = await _statisticsService.GetByYear(year);
            if(null == result)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            } 
            
            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(result);
            return response;
        }

        [GeneratedRegex("\\d{4}")]
        private static partial Regex YearRegEx();
    }
}
