using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.ScheduledJobs.Bindings
{
    public class RequiredPaymentAuditData
    {
        public SubmissionJobsToBeDeletedModel[] JobsToBeDeleted { get; set; }
    }
}