using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.ScheduledJobs.Common;

namespace SFA.DAS.Payments.ScheduledJobs.Services;

public class GsoAuditDataCleanUpDataService : IGsoAuditDataCleanUpDataService
{
    private readonly ILogger<GsoAuditDataCleanUpDataService> _logger;
    private readonly GsoPaymentsDataContext _paymentDataContext;

    public GsoAuditDataCleanUpDataService(GsoPaymentsDataContext paymentDataContext,
        ILogger<GsoAuditDataCleanUpDataService> logger)
    {
        _paymentDataContext = paymentDataContext;
        _logger = logger;
    }

    public async Task<IEnumerable<GsoJobsToBeDeletedBatch>> GetGsoDuplicateJobsToBeDeletedBatches(
        string collectionPeriod, string academicYear, byte gsoCourseType)
    {
        var selectExternalEarningsIdsToBeDeleted = @"
        IF OBJECT_ID('tempdb..#GsoJobsToBeDeleted') IS NOT NULL DROP TABLE #GsoJobsToBeDeleted;

        ;WITH RankedGsoRecords AS (
            SELECT
                ExternalEarningsId,
                ROW_NUMBER() OVER (
                    PARTITION BY Ukprn, LearnerReferenceNumber, CourseCode
                    ORDER BY ExternalEarningsId DESC
                ) AS RecencyRank
            FROM Payments2.RequiredPaymentEvent
            WHERE CourseType = @gsoCourseType
                AND CollectionPeriod = @collectionPeriod
                AND AcademicYear = @academicYear
        )
        SELECT DISTINCT ExternalEarningsId INTO #GsoJobsToBeDeleted
        FROM RankedGsoRecords
        WHERE RecencyRank > 1;

        SELECT ExternalEarningsId FROM #GsoJobsToBeDeleted";

        return (await _paymentDataContext.GsoExternalEarningsIds
                .FromSqlRaw(selectExternalEarningsIdsToBeDeleted,
                    new SqlParameter("gsoCourseType", gsoCourseType),
                    new SqlParameter("collectionPeriod", collectionPeriod),
                    new SqlParameter("academicYear", academicYear))
                .ToListAsync())
            .Select(row => row.ExternalEarningsId)
            .ToBatch(100);
    }
}
