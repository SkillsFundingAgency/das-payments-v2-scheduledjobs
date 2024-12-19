using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.ScheduledJobs.V1.Bindings;
using SFA.DAS.Payments.ScheduledJobs.V1.Services;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Functions
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
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "TriggerAuditDataCleanUp")] HttpRequestData httpRequest)
        {
            var response = httpRequest.CreateResponse();

            try
            {
                var result = await _auditDataCleanUpService.TriggerAuditDataCleanUp();

                if (result != null)
                {
                    response.StatusCode = HttpStatusCode.OK;
                    await response.WriteStringAsync("Request processed successfully");
                }
                else
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    await response.WriteStringAsync("No DCJobIds found for processing.");
                }

                return result;
            }
            catch (Exception ex)
            {
                string errorMessage = $"An error occurred while processing the request. {ex.Message}";
                _logger.LogError(errorMessage);
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync(errorMessage);
            }

            return null;
        }


        [Function("httpSBA")]
        public async Task<AuditDataCleanUpBinding> httpSBA(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "httpSBA")] HttpRequestData httpRequest)
        {
            var response = httpRequest.CreateResponse();

            try
            {
                await _auditDataCleanUpService.SendMessageToQueueAsync();

                await _auditDataCleanUpService.ReceiveMessagesFromQueueAsync();
            }
            catch (Exception ex)
            {
                string errorMessage = $"An error occurred while processing the request. {ex.Message}";
                _logger.LogError(errorMessage);
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync(errorMessage);
            }

            return null;
        }
    }
}
