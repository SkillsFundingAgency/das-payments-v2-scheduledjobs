using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Payments.FundingSource.Messages.Commands;

namespace SFA.DAS.Payments.ScheduledJobs.Bindings
{
    public class LevyAccountImportBinding
    {
        [ServiceBusOutput("%LevyAccountBalanceEndpoint%", Connection = "ServiceBusConnectionString")]
        public ImportEmployerAccounts LevyAccountImport { get; set; }
    }
}
