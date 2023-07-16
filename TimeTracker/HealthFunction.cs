using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace TimeTracker
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
            _logger.LogInformation("C# HTTP trigger function processed a request.");

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
