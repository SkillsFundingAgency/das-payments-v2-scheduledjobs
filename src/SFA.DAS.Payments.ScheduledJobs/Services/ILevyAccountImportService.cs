using SFA.DAS.Payments.ScheduledJobs.Bindings;

namespace SFA.DAS.Payments.ScheduledJobs.Services
{
    public interface ILevyAccountImportService
    {
        LevyAccountImportBinding RunLevyAccountImport();
    }
}