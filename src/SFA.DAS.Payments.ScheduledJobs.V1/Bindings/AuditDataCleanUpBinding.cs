using Microsoft.Azure.Functions.Worker;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Bindings
{
    public class AuditDataCleanUpBinding
    {
        [ServiceBusOutput("%DataLockAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
        public DataLockAuditDataCleanUpBinding DataLock { get; set; }

        [ServiceBusOutput("%EarningAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
        public EarningAuditDataCleanUpBinding EarningAudit { get; set; }

        [ServiceBusOutput("%FundingSourceAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
        public FundingSourceAuditDataCleanUpBinding FundingSource { get; set; }

        [ServiceBusOutput("%RequiredPaymentAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
        public RequiredPaymentAuditDataCleanUpBinding RequiredPayments { get; set; }
    }
}
