using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.ScheduledJobs.V1.Common;
using SFA.DAS.Payments.ScheduledJobs.V1.DTOS;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Services
{
    public class AuditDataCleanUpDataservice : IAuditDataCleanUpDataservice
    {
        private readonly IPaymentsDataContext _PaymentDataContext;
        private readonly ILogger<AuditDataCleanUpService> _logger;

        public AuditDataCleanUpDataservice(IPaymentsDataContext paymentDataContext, ILogger<AuditDataCleanUpService> logger)
        {
            _PaymentDataContext = paymentDataContext;
            _logger = logger;
        }

        public async Task<IEnumerable<SubmissionJobsToBeDeletedBatch>> GetSubmissionJobsToBeDeletedBatches(string collectionPeriod, string academicYear)
        {
            // ReSharper disable once ConvertToConstant.Local
            var selectJobsToBeDeleted = @"
                IF OBJECT_ID('tempdb..#JobDataToBeDeleted') IS NOT NULL DROP TABLE #JobDataToBeDeleted;

                SELECT JobId INTO #JobDataToBeDeleted FROM Payments2.EarningEvent
                WHERE CollectionPeriod = @collectionPeriod AND AcademicYear = @academicYear
                UNION
                SELECT JobId FROM Payments2.RequiredPaymentEvent
                WHERE CollectionPeriod = @collectionPeriod AND AcademicYear = @academicYear
                UNION
                SELECT JobId FROM Payments2.FundingSourceEvent
                WHERE CollectionPeriod = @collectionPeriod AND AcademicYear = @academicYear
                UNION
                SELECT JobId FROM Payments2.DataLockEvent
                WHERE CollectionPeriod = @collectionPeriod AND AcademicYear = @academicYear;

                -- keep all successful Jobs, 
                DELETE FROM #JobDataToBeDeleted WHERE JobId IN ( SELECT DcJobId FROM Payments2.LatestSuccessfulJobs );

                -- and Keep all in progress and Timed-out and Failed on DC Jobs
                DELETE FROM #JobDataToBeDeleted WHERE JobId IN ( SELECT DcJobId FROM Payments2.Job WHERE [Status] in (1, 4, 5) );

                -- and any jobs completed on our side but DC status is unknown
                DELETE FROM #JobDataToBeDeleted WHERE JobId IN ( SELECT DcJobId FROM Payments2.Job Where [Status] in (2, 3) AND Dcjobsucceeded IS NULL);

                SELECT JobId AS DcJobId FROM #JobDataToBeDeleted";

            return (await _PaymentDataContext.SubmissionJobsToBeDeleted
                    .FromSqlRaw(selectJobsToBeDeleted,
                        new SqlParameter("collectionPeriod", collectionPeriod),
                        new SqlParameter("academicYear", academicYear))
                    .ToListAsync())
                .ToBatch(100);
        }
    }
}
