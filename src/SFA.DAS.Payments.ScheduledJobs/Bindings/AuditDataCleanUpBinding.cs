using Microsoft.Azure.Functions.Worker;

namespace SFA.DAS.Payments.ScheduledJobs.Bindings
{
    public class AuditDataCleanUpBinding
    {
        public AuditDataCleanUpBinding()
        {
            DataLock = new List<DataLockAuditData>();
            EarningAudit = new List<EarningAuditData>();
            FundingSource = new List<FundingSourceAuditData>();
            RequiredPayments = new List<RequiredPaymentAuditData>();
        }

        [ServiceBusOutput("%DataLockAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
        public List<DataLockAuditData> DataLock { get; set; }

        [ServiceBusOutput("%EarningAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
        public List<EarningAuditData> EarningAudit { get; set; }

        [ServiceBusOutput("%FundingSourceAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
        public List<FundingSourceAuditData> FundingSource { get; set; }

        [ServiceBusOutput("%RequiredPaymentAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
        public List<RequiredPaymentAuditData> RequiredPayments { get; set; }
    }
}
