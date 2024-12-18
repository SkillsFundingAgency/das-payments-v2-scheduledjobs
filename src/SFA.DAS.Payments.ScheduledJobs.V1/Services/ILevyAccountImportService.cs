using SFA.DAS.Payments.FundingSource.Messages.Commands;
using SFA.DAS.Payments.ScheduledJobs.V1.Bindings;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Services
{
    public interface ILevyAccountImportService
    {
        LevyAccountImportBinding RunLevyAccountImport();
    }
}