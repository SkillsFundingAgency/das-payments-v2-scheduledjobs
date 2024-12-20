using Microsoft.Azure.Functions.Worker;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Bindings
{
    public class AuditDataCleanUpBinding
    {
        [ServiceBusOutput("%DataLockAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
        public DataLockAuditData DataLock { get; set; }

        [ServiceBusOutput("%EarningAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
        public EarningAuditData EarningAudit { get; set; }

        [ServiceBusOutput("%FundingSourceAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
        public FundingSourceAuditData FundingSource { get; set; }

        [ServiceBusOutput("%RequiredPaymentAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
        public RequiredPaymentAuditData RequiredPayments { get; set; }
    }
}
