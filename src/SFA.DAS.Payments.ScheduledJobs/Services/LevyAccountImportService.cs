using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.FundingSource.Messages.Commands;
using SFA.DAS.Payments.ScheduledJobs.Bindings;

namespace SFA.DAS.Payments.ScheduledJobs.Services
{
    public class LevyAccountImportService : ILevyAccountImportService
    {
        private ILogger<LevyAccountImportService> _logger;
        public LevyAccountImportService(ILogger<LevyAccountImportService> logger)
        {
            _logger = logger;
        }

        public LevyAccountImportBinding RunLevyAccountImport()
        {
            return new LevyAccountImportBinding
            {
                EventId = new ImportEmployerAccounts().EventId
            };
        }
    }
}
