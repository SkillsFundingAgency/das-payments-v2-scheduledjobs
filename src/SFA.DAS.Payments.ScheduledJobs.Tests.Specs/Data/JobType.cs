namespace SFA.DAS.Payments.ScheduledJobs.Tests.Specs.Data
{
    public enum JobType : byte
    {
        EarningsJob = 1,
        PeriodEndStartJob,
        ComponentAcceptanceTestEarningsJob,
        ComponentAcceptanceTestMonthEndJob,
        PeriodEndRunJob,
        PeriodEndStopJob,
        PeriodEndSubmissionWindowValidationJob,
        PeriodEndRequestReportsJob,
        PeriodEndIlrReprocessingJob,
        PeriodEndFcsHandOverCompleteJob
    }
}