using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.ScheduledJobs.Dtos
{
    public class SubmissionJobsToBeDeletedBatch
    {
        public SubmissionJobsToBeDeletedModel[] JobsToBeDeleted { get; set; }
    }
}
