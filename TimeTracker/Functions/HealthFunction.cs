using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace TimeTracker.Functions
{
    public class HealthFunction
    {
        private readonly ILogger _logger;

        public HealthFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HealthFunction>();
        }

        [Function("health")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req)
        {
            _logger.HealthFunctionExecuting();

            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(new
            {
                Application = "TimeTracker",
                Status = "healthy"
            });
            return response;
        }
    }

    internal static class HealthFunctionLoggerExtensions
    {
        private static readonly Action<ILogger, Exception?> _healthFunctionExecuting;

        static HealthFunctionLoggerExtensions()
        {
            _healthFunctionExecuting = LoggerMessage.Define(
                logLevel: LogLevel.Debug,
                eventId: 1,
                formatString: "HealthFunction is processing a HTTP trigger");
        }

        public static void HealthFunctionExecuting(this ILogger logger)
        {
            _healthFunctionExecuting(logger, null);
        }
    }
}
