using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Payments.ScheduledJobs.Services;

namespace SFA.DAS.Payments.ScheduledJobs.Functions
{
    public class GsoRequiredPaymentAuditDataCleanUp
    {
        private readonly ILogger<GsoRequiredPaymentAuditDataCleanUp> _logger;
        private readonly IGsoAuditDataCleanUpService _gsoAuditDataCleanUpService;

        public GsoRequiredPaymentAuditDataCleanUp(ILogger<GsoRequiredPaymentAuditDataCleanUp> logger, IGsoAuditDataCleanUpService gsoAuditDataCleanUpService)
        {
            _logger = logger;
            _gsoAuditDataCleanUpService = gsoAuditDataCleanUpService;
        }

        [Function(nameof(GsoRequiredPaymentAuditDataCleanUp))]
        public async Task Run(
            [ServiceBusTrigger("%GsoRequiredPaymentAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var batch = JsonConvert.DeserializeObject<GsoJobsToBeDeletedBatch>(message.Body.ToString());

            await _gsoAuditDataCleanUpService.RequiredPaymentGsoAuditDataCleanUp(batch);
        }
    }
}
