using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.ScheduledJobs.V1.Functions;

namespace SFA.DAS.Payments.ScheduledJobs.V1.DTOS
{
    public class SubmissionJobsToBeDeletedBatch
    {
        [ServiceBusOutput("%EarningAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
        public SubmissionJobsToBeDeletedModel[] JobsToBeDeleted { get; set; }
    }
}
