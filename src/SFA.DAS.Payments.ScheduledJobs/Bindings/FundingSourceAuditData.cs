using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.ScheduledJobs.Bindings
{
    public class FundingSourceAuditData
    {
        public SubmissionJobsToBeDeletedModel[] JobsToBeDeleted { get; set; }
    }
}