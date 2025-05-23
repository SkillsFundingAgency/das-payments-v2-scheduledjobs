using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.ScheduledJobs.Bindings;
using System.Net;

namespace SFA.DAS.Payments.ScheduledJobs.Functions
{
    public class AuditDataCleanUpTrigger
    {
        private readonly ILogger<AuditDataCleanUpTrigger> _logger;
        private readonly IAuditDataCleanUpService _auditDataCleanUpService;

        public AuditDataCleanUpTrigger(IAuditDataCleanUpService auditDataCleanUpService
            , ILogger<AuditDataCleanUpTrigger> logger)
        {
            _auditDataCleanUpService = auditDataCleanUpService;
            _logger = logger;
        }

        [Function("AuditDataCleanUpTrigger")]
        public async Task<AuditDataCleanUpBinding> AuditDataCleanUp([TimerTrigger("%AuditDataCleanUpSchedule%")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            if (myTimer.ScheduleStatus is not null)
            {
                return await _auditDataCleanUpService.TriggerAuditDataCleanUp();
            }
            return null;
        }

        [Function("HttpTriggerAuditDataCleanUp")]
        public async Task<AuditDataCleanUpBinding> HttpTriggerAuditDataCleanUp(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "TriggerAuditDataCleanUp")] HttpRequest httpRequest)
        {
            var response = httpRequest.HttpContext.Response;
            try
            {
                var result = await _auditDataCleanUpService.TriggerAuditDataCleanUp();

                if (result != null)
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                    await response.WriteAsync("Request processed successfully");
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await response.WriteAsync("No DCJobIds found for processing.");
                }

                return result;
            }
            catch (Exception ex)
            {
                string errorMessage = $"An error occurred while processing the request. {ex.Message}";
                _logger.LogError(errorMessage);
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await response.WriteAsync(errorMessage);
            }

            return null;
        }

    }
}
