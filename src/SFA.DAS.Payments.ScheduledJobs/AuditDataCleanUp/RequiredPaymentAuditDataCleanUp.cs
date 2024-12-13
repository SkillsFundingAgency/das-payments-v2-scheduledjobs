using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable UnusedMember.Global

namespace SFA.DAS.Payments.ScheduledJobs.AuditDataCleanUp
{
    public class RequiredPaymentAuditDataCleanUp
    {
        private readonly ILogger _logger;
        private readonly IAuditDataCleanUpService _auditDataCleanUpService;

        public RequiredPaymentAuditDataCleanUp(ILogger<RequiredPaymentAuditDataCleanUp> logger, IAuditDataCleanUpService auditDataCleanUpService)
        {
            _logger = logger;
            _auditDataCleanUpService = auditDataCleanUpService;
        }

        [Function("RequiredPaymentEventAuditDataCleanUp")]
        public async Task RequiredPaymentEventAuditDataCleanUp([ServiceBusTrigger("%RequiredPaymentAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")] string message)
        {
            var batch = JsonConvert.DeserializeObject<SubmissionJobsToBeDeletedBatch>(message);

            await _auditDataCleanUpService.RequiredPaymentEventAuditDataCleanUp(batch);
        }
    }
}