using Microsoft.Azure.Functions.Worker;

namespace SFA.DAS.Payments.ScheduledJobs.Bindings
{
    public class LevyAccountImportBinding
    {
        [ServiceBusOutput("%LevyAccountBalanceEndpoint%", Connection = "ServiceBusConnectionString")]
        public Guid EventId { get; set; }
    }
}
