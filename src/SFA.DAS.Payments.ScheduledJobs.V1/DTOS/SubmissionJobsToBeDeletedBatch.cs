using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.ScheduledJobs.V1.DTOS
{
    public class SubmissionJobsToBeDeletedBatch
    {
        [ServiceBusOutput("%EarningAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
        public SubmissionJobsToBeDeletedModel[] JobsToBeDeleted { get; set; }
    }
}
