using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TimeTracker.Model;
using TimeTracker.Service;

namespace TimeTracker.Functions.LogEntries
{
    public class FindLogEntiesForYearAndProject
    {
        private static readonly Action<ILogger, string, Exception?> _searchFunctionExecuting = LoggerMessage
            .Define<string>(logLevel: LogLevel.Debug, eventId: 4, formatString: "SearchFunction is processing a HTTP trigger with body {Body}");

        private readonly ILogger<FindLogEntiesForYearAndProject> _logger;
        private readonly IEntryService _entryService;

        public FindLogEntiesForYearAndProject(ILogger<FindLogEntiesForYearAndProject> logger, IEntryService entryService)
        {
            _logger = logger;
            _entryService = entryService;
        }

        [Function("FindLogEntiesForYearAndProject")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "search")] HttpRequestData req)
        {
            var requestBody = string.Empty;
            using (StreamReader streamReader = new(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            var requestEntry = JsonConvert.DeserializeObject<SearchRequest>(requestBody);
            _searchFunctionExecuting.Invoke(_logger, requestBody, null);

            if (null != requestEntry && requestEntry.Validate())
            {
                var result = await _entryService.FindLogEntriesByYearAndProject(requestEntry.Year, requestEntry.Project, requestEntry.Query);
                var response = req.CreateResponse();
                await response.WriteAsJsonAsync(result);
                return response;
            }
            else
            {
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}
