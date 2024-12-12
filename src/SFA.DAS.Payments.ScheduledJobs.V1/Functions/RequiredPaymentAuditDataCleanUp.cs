using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Payments.ScheduledJobs.V1.DTOS;
using SFA.DAS.Payments.ScheduledJobs.V1.Services;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Functions
{
    public class RequiredPaymentAuditDataCleanUp
    {
        private readonly ILogger<RequiredPaymentAuditDataCleanUp> _logger;
        private readonly IAuditDataCleanUpService _auditDataCleanUpService;
        public RequiredPaymentAuditDataCleanUp(ILogger<RequiredPaymentAuditDataCleanUp> logger, IAuditDataCleanUpService auditDataCleanUpService)
        {
            _logger = logger;
            _auditDataCleanUpService = auditDataCleanUpService;
        }

        [Function(nameof(RequiredPaymentAuditDataCleanUp))]
        public async Task Run(
            [ServiceBusTrigger("%RequiredPaymentAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var batch = JsonConvert.DeserializeObject<SubmissionJobsToBeDeletedBatch>(message.Subject);

            await _auditDataCleanUpService.RequiredPaymentEventAuditDataCleanUp(batch);
        }
    }
}
