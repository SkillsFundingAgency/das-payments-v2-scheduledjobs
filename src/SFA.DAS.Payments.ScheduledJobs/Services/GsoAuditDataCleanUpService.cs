using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.ScheduledJobs.Common;

namespace SFA.DAS.Payments.ScheduledJobs.Services
{
    public class GsoAuditDataCleanUpService : IGsoAuditDataCleanUpService
    {
        private const byte GsoCourseType = (byte)CourseType.ShortCourse;

        private readonly ILogger<GsoAuditDataCleanUpService> _logger;
        private readonly IPaymentsDataContext _paymentDataContext;
        private readonly IServiceBusClientHelper _serviceBusClientHelper;
        private readonly IGsoAuditDataCleanUpDataService _gsoAuditDataCleanUpDataService;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        public GsoAuditDataCleanUpService(IPaymentsDataContext dataContext
            , ILogger<GsoAuditDataCleanUpService> paymentLogger
            , IServiceBusClientHelper serviceBusClientHelper,
            IGsoAuditDataCleanUpDataService gsoAuditDataCleanUpDataService,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            _paymentDataContext = dataContext;
            _logger = paymentLogger;
            _serviceBusClientHelper = serviceBusClientHelper;
            _gsoAuditDataCleanUpDataService = gsoAuditDataCleanUpDataService;
            _configuration = configuration;
            _environment = environment;
        }

        public async Task CleanUpGsoAuditData()
        {
            var previousAcademicYearCollectionPeriod = _configuration.GetConfigurationValue(_environment, "PreviousAcademicYearCollectionPeriod");
            var previousAcademicYear = _configuration.GetConfigurationValue(_environment, "PreviousAcademicYear");
            var currentCollectionPeriod = _configuration.GetConfigurationValue(_environment, "CurrentCollectionPeriod");
            var currentAcademicYear = _configuration.GetConfigurationValue(_environment, "CurrentAcademicYear");

            var previousGsoJobsToBeDeletedBatches = await GetGsoJobsToBeDeletedBatches(previousAcademicYearCollectionPeriod, previousAcademicYear);
            var currentGsoJobsToBeDeletedBatches = await GetGsoJobsToBeDeletedBatches(currentCollectionPeriod, currentAcademicYear);

            var gsoJobsToBeDeletedBatches = previousGsoJobsToBeDeletedBatches.Union(currentGsoJobsToBeDeletedBatches).ToList();

            _logger.LogInformation($"Triggering GSO Audit Data Cleanup for {gsoJobsToBeDeletedBatches.Count} submission job batches. " +
                                    $"ExternalEarningsIds: {string.Join(",", gsoJobsToBeDeletedBatches.SelectMany(x => x.ExternalEarningsIdsToBeDeleted))}");

            if (!gsoJobsToBeDeletedBatches.Any())
            {
                return;
            }

            string gsoRequiredPaymentAuditDataCleanUpQueue = _environment.IsDevelopment()
                ? _configuration.GetValue<string>("GsoRequiredPaymentAuditDataCleanUpQueue")
                : Environment.GetEnvironmentVariable("GsoRequiredPaymentAuditDataCleanUpQueue");

            foreach (var batch in gsoJobsToBeDeletedBatches)
            {
                foreach (var externalEarningsId in batch.ExternalEarningsIdsToBeDeleted)
                {
                    var individualRecord = new GsoJobsToBeDeletedBatch { ExternalEarningsIdsToBeDeleted = new[] { externalEarningsId } };
                    var jsonRecord = JsonConvert.SerializeObject(individualRecord);
                    await _serviceBusClientHelper.SendMessageToQueueAsync(gsoRequiredPaymentAuditDataCleanUpQueue, jsonRecord);
                }
            }
        }

        public async Task RequiredPaymentGsoAuditDataCleanUp(GsoJobsToBeDeletedBatch batch)
        {
            var externalEarningsId = batch.ExternalEarningsIdsToBeDeleted.First();

            try
            {
                _logger.LogInformation($"Started DeleteGsoRequiredPaymentEvent for ExternalEarningsId {externalEarningsId}");

                var requiredPaymentEventCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                    "DELETE Payments2.RequiredPaymentEvent WHERE ExternalEarningsId = @externalEarningsId",
                    new SqlParameter("externalEarningsId", externalEarningsId));

                _logger.LogInformation($"DELETED {requiredPaymentEventCount} RequiredPaymentEvents for ExternalEarningsId {externalEarningsId}");
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Error Deleting Gso Required Payment Audit Data, internal Exception {e}");
                throw;
            }
        }

        private async Task<IEnumerable<GsoJobsToBeDeletedBatch>> GetGsoJobsToBeDeletedBatches(string collectionPeriod, string academicYear)
        {
            if (!string.IsNullOrWhiteSpace(collectionPeriod) && !string.IsNullOrWhiteSpace(academicYear))
            {
                return await _gsoAuditDataCleanUpDataService.GetGsoDuplicateJobsToBeDeletedBatches(collectionPeriod, academicYear, GsoCourseType);
            }

            return Enumerable.Empty<GsoJobsToBeDeletedBatch>();
        }
    }
}
