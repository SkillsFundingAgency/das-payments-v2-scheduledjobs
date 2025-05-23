﻿using SFA.DAS.Payments.ScheduledJobs.Bindings;

namespace SFA.DAS.Payments.ScheduledJobs.Services
{
    public interface IAuditDataCleanUpService
    {
        Task DataLockEventAuditDataCleanUp(SubmissionJobsToBeDeletedBatch batch);
        Task EarningEventAuditDataCleanUp(SubmissionJobsToBeDeletedBatch batch);
        Task FundingSourceEventAuditDataCleanUp(SubmissionJobsToBeDeletedBatch batch);
        Task RequiredPaymentEventAuditDataCleanUp(SubmissionJobsToBeDeletedBatch batch);
        Task<AuditDataCleanUpBinding> TriggerAuditDataCleanUp();
    }
}