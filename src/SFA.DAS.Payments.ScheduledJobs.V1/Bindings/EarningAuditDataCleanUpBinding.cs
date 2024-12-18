using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Bindings
{
    public class EarningAuditDataCleanUpBinding
    {
        public SubmissionJobsToBeDeletedModel[] JobsToBeDeleted { get; set; }
    }
}
