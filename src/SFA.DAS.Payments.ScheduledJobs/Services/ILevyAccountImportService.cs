using SFA.DAS.Payments.FundingSource.Messages.Commands;

namespace SFA.DAS.Payments.ScheduledJobs.Services
{
    public interface ILevyAccountImportService
    {
        Task<ImportEmployerAccounts> RunLevyAccountImport();
    }
}