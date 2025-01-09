using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.ScheduledJobs.Bindings;
using SFA.DAS.Payments.ScheduledJobs.Services;

namespace SFA.DAS.Payments.ScheduledJobs.Functions
{
    public class LevyAccountImport
    {
        private readonly ILogger<LevyAccountImport> _logger;
        private readonly ILevyAccountImportService _levyAccountImportService;

        public LevyAccountImport(ILogger<LevyAccountImport> logger
            , ILevyAccountImportService levyAccountImportService)
        {
            _logger = logger;
            _levyAccountImportService = levyAccountImportService;
        }

        [Function("LevyAccountImport")]
        public LevyAccountImportBinding Run([TimerTrigger("%LevyAccountSchedule%")] TimerInfo myTimer)
        {
            return _levyAccountImportService.RunLevyAccountImport();
        }

        [Function("HttpLevyAccountImport")]
        public LevyAccountImportBinding HttpLevyAccountImport([HttpTrigger(AuthorizationLevel.Function, "get", Route = "ImportLevyAccount")] HttpRequest httpRequest)
        {
            var response = httpRequest.HttpContext.Response;

            try
            {
                var result = _levyAccountImportService.RunLevyAccountImport();
                response.WriteAsync("Request processed successfully");
                return result;
            }
            catch (Exception ex)
            {

                string errorMessage = $"An error occurred while processing the request. {ex.Message}";
                _logger.LogError(errorMessage);
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.WriteAsync(errorMessage);
            }
            return null;
        }
    }
}
