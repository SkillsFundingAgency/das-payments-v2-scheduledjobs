using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NServiceBus.Routing;
using SFA.DAS.Payments.ScheduledJobs.V1.Bindings;
using SFA.DAS.Payments.ScheduledJobs.V1.DTOS;
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
        public async Task AuditDataCleanUp([TimerTrigger("%AuditDataCleanUpSchedule%")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            if (myTimer.ScheduleStatus is not null)
            {
                await _auditDataCleanUpService.TriggerAuditDataCleanup();
            }

        }

        [Function("HttpTriggerAuditDataCleanUp")]
        public async Task<AuditDataCleanUpBinding> HttpTriggerAuditDataCleanUp(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "TriggerAuditDataCleanUp")] HttpRequestData httpRequest)
        {
            var response = httpRequest.CreateResponse();
            var result = await _auditDataCleanUpService.TriggerAuditDataCleanup();

            if (result != null)
            {
                response.StatusCode = System.Net.HttpStatusCode.OK;
                await response.WriteStringAsync("Request processed successfully");
            }
            else
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                await response.WriteStringAsync("Request processing failed");
            }

            return result;
        }
    }
}
