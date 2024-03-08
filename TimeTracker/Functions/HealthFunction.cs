using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace TimeTracker.Functions
{
    public class HealthFunction
    {
        private static readonly Action<ILogger, Exception?> _healthFunctionExecuting = LoggerMessage
            .Define(logLevel: LogLevel.Debug, eventId: 1, formatString: "HealthFunction is processing a HTTP trigger");

        private readonly ILogger _logger;

        public HealthFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HealthFunction>();
        }

        [Function("health")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req)
        {
            _healthFunctionExecuting.Invoke(_logger, null);

            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(new
            {
                Application = "TimeTracker",
                Status = "healthy"
            });
            return response;
        }
    }
}
