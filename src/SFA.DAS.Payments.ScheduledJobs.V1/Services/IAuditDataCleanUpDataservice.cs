using SFA.DAS.Payments.ScheduledJobs.V1.DTOS;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Services
{
    public interface IAuditDataCleanUpDataservice
    {
        Task<IEnumerable<SubmissionJobsToBeDeletedBatch>> GetSubmissionJobsToBeDeletedBatches(string collectionPeriod, string academicYear);
    }
}
