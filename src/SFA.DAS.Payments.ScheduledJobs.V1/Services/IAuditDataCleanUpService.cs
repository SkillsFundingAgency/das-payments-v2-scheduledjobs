using SFA.DAS.Payments.ScheduledJobs.V1.DTOS;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Services
{
    public interface IAuditDataCleanUpService
    {
        Task DataLockEventAuditDataCleanUp(SubmissionJobsToBeDeletedBatch batch);
        Task EarningEventAuditDataCleanUp(SubmissionJobsToBeDeletedBatch batch);
        Task FundingSourceEventAuditDataCleanUp(SubmissionJobsToBeDeletedBatch batch);
        Task RequiredPaymentEventAuditDataCleanUp(SubmissionJobsToBeDeletedBatch batch);
        Task TriggerAuditDataCleanup();
    }
}