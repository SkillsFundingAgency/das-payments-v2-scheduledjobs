using SFA.DAS.Payments.ScheduledJobs.DTOS;

namespace SFA.DAS.Payments.ScheduledJobs.Services
{
    public interface IAuditDataCleanUpDataservice
    {
        Task<IEnumerable<SubmissionJobsToBeDeletedBatch>> GetSubmissionJobsToBeDeletedBatches(string collectionPeriod, string academicYear);
    }
}
