using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.Payments.ScheduledJobs.Infrastructure.IoC;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;

// ReSharper disable UnusedMember.Global

namespace SFA.DAS.Payments.ScheduledJobs.AuditDataCleanUp
{
    public class FundingSourceAuditDataCleanUp
    {
        private readonly ILogger _logger;
        private readonly IAuditDataCleanUpService _auditDataCleanUpService;

        public FundingSourceAuditDataCleanUp(ILogger<FundingSourceAuditDataCleanUp> logger, IAuditDataCleanUpService auditDataCleanUpService)
        {
            _logger = logger;
            _auditDataCleanUpService = auditDataCleanUpService;
        }

        [Function("FundingSourceEventAuditDataCleanUp")]
        public async Task FundingSourceEventAuditDataCleanUp([ServiceBusTrigger("%FundingSourceAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")] string message)
        {
            var batch = JsonConvert.DeserializeObject<SubmissionJobsToBeDeletedBatch>(message);

            await _auditDataCleanUpService.FundingSourceEventAuditDataCleanUp(batch);
        }
    }
}