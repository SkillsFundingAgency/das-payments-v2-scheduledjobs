using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Bindings
{
    public class AuditDataCleanUpBinding
    {

        [ServiceBusOutput("%DataLockAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
        public SubmissionJobsToBeDeletedModel[] DataLockAuditDataCleanUpJobsToBeDeleted { get; set; }

        [ServiceBusOutput("%EarningAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
        public SubmissionJobsToBeDeletedModel[] EarningAuditDataCleanUpJobsToBeDeleted { get; set; }

        [ServiceBusOutput("%FundingSourceAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
        public SubmissionJobsToBeDeletedModel[] FundingSourceAuditDataCleanUpJobsToBeDeleted { get; set; }

        [ServiceBusOutput("%RequiredPaymentAuditDataCleanUpQueue%", Connection = "ServiceBusConnectionString")]
        public SubmissionJobsToBeDeletedModel[] RequiredPaymentAuditDataCleanUpJobsToBeDeleted { get; set; }
    }
}
