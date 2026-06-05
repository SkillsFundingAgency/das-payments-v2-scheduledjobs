using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.FundingSource.Messages.Commands;
using SFA.DAS.Payments.ScheduledJobs.Bindings;
using System.Net;

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
        public async Task Run([TimerTrigger("%LevyAccountSchedule%")] TimerInfo myTimer)
        {
            try
            {
                await _levyAccountImportService.RunLevyAccountImport();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while processing the scheduled levy account import. {ex.Message}");
            }
        }

        [Function("HttpLevyAccountImport")]
        public async Task<ImportEmployerAccounts> HttpLevyAccountImport([HttpTrigger(AuthorizationLevel.Function, "get", Route = "ImportLevyAccount")] HttpRequest httpRequest)
        {
            try
            {
                return await _levyAccountImportService.RunLevyAccountImport();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while processing the request. {ex.Message}");
                var response = httpRequest.HttpContext.Response;
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            return null;
        }
    }
}
