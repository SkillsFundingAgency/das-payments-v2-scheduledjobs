using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Bindings
{
    public class LevyAccountImportBinding
    {
        [ServiceBusOutput("%LevyAccountBalanceEndpoint%", Connection = "ServiceBusConnectionString")]
        public Guid EventId { get; set; }
    }
}
