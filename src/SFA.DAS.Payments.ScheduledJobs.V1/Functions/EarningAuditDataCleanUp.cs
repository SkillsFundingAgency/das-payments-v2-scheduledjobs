using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Payments.ScheduledJobs.V1.DTOS;
using SFA.DAS.Payments.ScheduledJobs.V1.Services;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Functions
{
    public class EarningAuditDataCleanUp
    {
        private readonly ILogger<EarningAuditDataCleanUp> _logger;
        private readonly IAuditDataCleanUpService _auditDataCleanUpService;
        public EarningAuditDataCleanUp(ILogger<EarningAuditDataCleanUp> logger, IAuditDataCleanUpService auditDataCleanUpService)
        {
            _logger = logger;
            _auditDataCleanUpService = auditDataCleanUpService;
        }

        [Function(nameof(EarningAuditDataCleanUp))]
        public async Task Run(
            [ServiceBusTrigger("%EarningAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var batch = JsonConvert.DeserializeObject<SubmissionJobsToBeDeletedBatch>(message.Body.ToString());

            await _auditDataCleanUpService.EarningEventAuditDataCleanUp(batch);
        }
    }
}
