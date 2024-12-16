using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NServiceBus.Routing;
using SFA.DAS.Payments.ScheduledJobs.V1.DTOS;
using SFA.DAS.Payments.ScheduledJobs.V1.Services;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Functions
{
    public class AuditDataCleanUpTrigger
    {
        private readonly ILogger<AuditDataCleanUpTrigger> _logger;
        private readonly IAuditDataCleanUpService _auditDataCleanUpService;

        public AuditDataCleanUpTrigger(IAuditDataCleanUpService auditDataCleanUpService
            , ILogger<AuditDataCleanUpTrigger> logger)
        {
            _auditDataCleanUpService = auditDataCleanUpService;
            _logger = logger;
        }

        [Function("AuditDataCleanUpTrigger")]
        public async Task<SubmissionJobsToBeDeletedBatch> AuditDataCleanUp([TimerTrigger("%AuditDataCleanUpSchedule%")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            if (myTimer.ScheduleStatus is not null)
            {
                var ListSubmissionJobsToBeDeletedBatch = await _auditDataCleanUpService.TriggerAuditDataCleanup();
                var JobsToBeDeleted = new SubmissionJobsToBeDeletedBatch()
                {
                    JobsToBeDeleted = ListSubmissionJobsToBeDeletedBatch.SelectMany(batch => batch.JobsToBeDeleted).ToArray()
                };

                return JobsToBeDeleted;
            }

            return null;
        }

        [Function("HttpTriggerAuditDataCleanup")]
        public async Task<SubmissionJobsToBeDeletedBatch> HttpTriggerAuditDataCleanup(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest httpRequest)
        {
            var ListSubmissionJobsToBeDeletedBatch = await _auditDataCleanUpService.TriggerAuditDataCleanup();
            var JobsToBeDeleted = new SubmissionJobsToBeDeletedBatch()
            {
                JobsToBeDeleted = ListSubmissionJobsToBeDeletedBatch.SelectMany(batch => batch.JobsToBeDeleted).ToArray()
            };

            return JobsToBeDeleted;
        }
    }
}
