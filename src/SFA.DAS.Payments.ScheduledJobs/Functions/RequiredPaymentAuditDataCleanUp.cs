using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SFA.DAS.Payments.ScheduledJobs.Functions
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
            var batch = JsonConvert.DeserializeObject<SubmissionJobsToBeDeletedBatch>(message.Body.ToString());

            await _auditDataCleanUpService.RequiredPaymentEventAuditDataCleanUp(batch);
        }
    }
}
