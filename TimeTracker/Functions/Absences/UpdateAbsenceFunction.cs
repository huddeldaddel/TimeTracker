using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TimeTracker.Model;
using TimeTracker.Service;

namespace TimeTracker.Functions.Absences
{
    public class UpdateAbsenceFunction
    {
        private readonly ILogger _logger;
        private readonly IAbsenceService _absenceService;

        public UpdateAbsenceFunction(ILoggerFactory loggerFactory, IAbsenceService absenceService)
        {
            _logger = loggerFactory.CreateLogger<UpdateAbsenceFunction>();
            _absenceService = absenceService;
        }

        [Function("UpdateAbsenceFunction")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "put", Route = "absences")] HttpRequestData req)
        {
            var requestBody = string.Empty;
            using (StreamReader streamReader = new(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            var requestEntry = JsonConvert.DeserializeObject<UpdateAbsenceRequest>(requestBody);
            if (null != requestEntry && requestEntry.Validate())
            {
                var result = await _absenceService.UpdateAbsence(requestEntry.ToAbsence()!);
                var response = req.CreateResponse();
                await response.WriteAsJsonAsync(result);
                return response;
            }

            return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
