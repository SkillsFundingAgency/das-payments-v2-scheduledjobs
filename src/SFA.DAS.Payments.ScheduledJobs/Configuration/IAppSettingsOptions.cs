namespace SFA.DAS.Payments.ScheduledJobs.Configuration
{
    public interface IAppSettingsOptions
    {
        ConnectionStrings ConnectionStrings { get; set; }
        string IsEncrypted { get; set; }
        Values Values { get; set; }
    }
}