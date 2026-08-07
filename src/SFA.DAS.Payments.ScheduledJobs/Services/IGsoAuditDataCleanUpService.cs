using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.ScheduledJobs.Services
{
    public interface IGsoAuditDataCleanUpService
    {
        Task CleanUpGsoAuditData();
        Task RequiredPaymentGsoAuditDataCleanUp(GsoJobsToBeDeletedBatch batch);
    }
}
