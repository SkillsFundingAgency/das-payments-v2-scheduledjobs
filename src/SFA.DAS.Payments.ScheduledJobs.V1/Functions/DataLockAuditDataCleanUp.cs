using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Payments.ScheduledJobs.V1.DTOS;
using SFA.DAS.Payments.ScheduledJobs.V1.Services;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Functions
{
    public class DataLockAuditDataCleanUp
    {
        private readonly ILogger<DataLockAuditDataCleanUp> _logger;
        private readonly IAuditDataCleanUpService _auditDataCleanUpService;
        public DataLockAuditDataCleanUp(ILogger<DataLockAuditDataCleanUp> logger, IAuditDataCleanUpService auditDataCleanUpService)
        {
            _logger = logger;
            _auditDataCleanUpService = auditDataCleanUpService;
        }

        [Function(nameof(DataLockAuditDataCleanUp))]
        public async Task Run(
            [ServiceBusTrigger("%DataLockAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            try
            {
                var batch = JsonConvert.DeserializeObject<SubmissionJobsToBeDeletedBatch>(message.Body.ToString());

                await _auditDataCleanUpService.DataLockEventAuditDataCleanUp(batch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the message: {MessageId}", message.MessageId);
            }
        }
    }
}
