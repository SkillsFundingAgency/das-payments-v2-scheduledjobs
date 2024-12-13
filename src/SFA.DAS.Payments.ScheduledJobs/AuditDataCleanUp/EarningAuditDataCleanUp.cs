using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable UnusedMember.Global

namespace SFA.DAS.Payments.ScheduledJobs.AuditDataCleanUp
{
    public class EarningAuditDataCleanUp
    {
        private readonly ILogger _logger;
        private readonly IAuditDataCleanUpService _auditDataCleanUpService;

        public EarningAuditDataCleanUp(ILogger<EarningAuditDataCleanUp> logger, IAuditDataCleanUpService auditDataCleanUpService)
        {
            _logger = logger;
            _auditDataCleanUpService = auditDataCleanUpService;
        }

        [Function("EarningEventAuditDataCleanUp")]
        public async Task EarningEventAuditDataCleanUp([ServiceBusTrigger("%EarningAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")] string message)
        {
            var batch = JsonConvert.DeserializeObject<SubmissionJobsToBeDeletedBatch>(message);

            await _auditDataCleanUpService.EarningEventAuditDataCleanUp(batch);
        }
    }
}