namespace SFA.DAS.Payments.ScheduledJobs.Configuration
{
    public interface IAppSettingsOptions
    {
        Connectionstrings ConnectionStrings { get; set; }
        bool IsEncrypted { get; set; }
        Values Values { get; set; }
    }
}