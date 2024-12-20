namespace SFA.DAS.Payments.ScheduledJobs.V1.Configuration
{
    public interface IAppsettingsOptions
    {
        Connectionstrings ConnectionStrings { get; set; }
        bool IsEncrypted { get; set; }
        Values Values { get; set; }
    }
}