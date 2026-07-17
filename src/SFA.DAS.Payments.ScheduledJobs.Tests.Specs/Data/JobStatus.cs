namespace SFA.DAS.Payments.ScheduledJobs.Tests.Specs.Data
{
    public enum JobStatus : byte
    {
        InProgress = 1,
        Completed,
        CompletedWithErrors,
        TimedOut,
        DcTasksFailed,
        PaymentsTaskFailed
    }
}