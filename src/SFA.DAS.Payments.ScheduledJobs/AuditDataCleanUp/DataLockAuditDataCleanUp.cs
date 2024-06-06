using System.Threading.Tasks;
using AzureFunctions.Autofac;
using Newtonsoft.Json;
using SFA.DAS.Payments.ScheduledJobs.Infrastructure.IoC;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;

// ReSharper disable UnusedMember.Global

namespace SFA.DAS.Payments.ScheduledJobs.AuditDataCleanUp
{
    [DependencyInjectionConfig(typeof(DependencyInjectionConfig))]
    public class DataLockAuditDataCleanUp
    {
        private readonly ILogger _logger;
        private readonly IAuditDataCleanUpService _auditDataCleanUpService;

        public DataLockAuditDataCleanUp(ILogger<DataLockAuditDataCleanUp> logger, IAuditDataCleanUpService auditDataCleanUpService)
        {
            _logger = logger;
            _auditDataCleanUpService = auditDataCleanUpService;
        }

        [Function("DataLockEventAuditDataCleanUp")]
        public async Task DataLockEventAuditDataCleanUp([ServiceBusTrigger("%DataLockAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")] string message)
        {
            var batch = JsonConvert.DeserializeObject<SubmissionJobsToBeDeletedBatch>(message);

            await _auditDataCleanUpService.DataLockEventAuditDataCleanUp(batch);
        }
    }
}