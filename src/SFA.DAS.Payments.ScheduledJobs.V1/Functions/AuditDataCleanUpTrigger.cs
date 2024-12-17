using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
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

        //[Function("AuditDataCleanUpTrigger")]
        //public async Task AuditDataCleanUp([TimerTrigger("%AuditDataCleanUpSchedule%")] TimerInfo myTimer)
        //{
        //    _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        //    if (myTimer.ScheduleStatus is not null)
        //    {
        //        await _auditDataCleanUpService.TriggerAuditDataCleanup();
        //    }

        //}

        [Function("HttpTriggerAuditDataCleanup")]
        public async Task<AuditDataCleanUpBinding> HttpTriggerAuditDataCleanup(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest httpRequest)
        {
            return await _auditDataCleanUpService.TriggerAuditDataCleanup();
        }
    }
}
