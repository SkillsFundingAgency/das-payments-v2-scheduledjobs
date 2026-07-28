namespace SFA.DAS.Payments.ScheduledJobs.Services
{
    public interface IGsoAuditDataCleanUpDataService
    {
        Task<IEnumerable<GsoJobsToBeDeletedBatch>> GetGsoDuplicateJobsToBeDeletedBatches(string collectionPeriod, string academicYear, byte gsoCourseType);
    }
}
