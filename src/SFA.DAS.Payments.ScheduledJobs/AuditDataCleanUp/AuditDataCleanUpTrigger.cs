using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SFA.DAS.Payments.ScheduledJobs.Infrastructure.IoC;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;

// ReSharper disable UnusedMember.Global

namespace SFA.DAS.Payments.ScheduledJobs.AuditDataCleanUp
{
    public class AuditDataCleanUpTrigger
    {
        private readonly ILogger _logger;
        private readonly IAuditDataCleanUpService _auditDataCleanUpService;

        public AuditDataCleanUpTrigger(ILogger<AuditDataCleanUpTrigger> logger, IAuditDataCleanUpService auditDataCleanUpService)
        {
            _logger = logger;
            _auditDataCleanUpService = auditDataCleanUpService;
        }

        [Function("TimerTriggerAuditDataCleanup")]
        public async Task TimerTriggerAuditDataCleanup(
            [TimerTrigger("%AuditDataCleanUpSchedule%", RunOnStartup = false)] TimerInfo timerInfo)
        {
            await _auditDataCleanUpService.TriggerAuditDataCleanup();
        }

        [Function("HttpTriggerAuditDataCleanup")]
        public async Task HttpTriggerAuditDataCleanup(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest httpRequest)
        {
            await _auditDataCleanUpService.TriggerAuditDataCleanup();
        }
    }
}
