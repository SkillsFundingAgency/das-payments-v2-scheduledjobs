using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.ScheduledJobs.Bindings;

namespace SFA.DAS.Payments.ScheduledJobs.Services
{
    public class AuditDataCleanUpService : IAuditDataCleanUpService
    {
        private readonly IPaymentsDataContext _paymentDataContext;
        private readonly ILogger<AuditDataCleanUpService> _logger;
        private readonly IServiceBusClientHelper _serviceBusClientHelper;
        private readonly IAuditDataCleanUpDataservice _auditDataCleanUpDataservice;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;


        public AuditDataCleanUpService(IPaymentsDataContext dataContext
            , ILogger<AuditDataCleanUpService> paymentLogger
            , IServiceBusClientHelper serviceBusClientHelper,
              IAuditDataCleanUpDataservice auditDataCleanUpDataservice,
              IConfiguration configuration,
              IHostEnvironment environment)
        {
            _paymentDataContext = dataContext;
            _logger = paymentLogger;
            _serviceBusClientHelper = serviceBusClientHelper;
            _auditDataCleanUpDataservice = auditDataCleanUpDataservice;
            _configuration = configuration;
            _environment = environment;
        }

        public async Task<AuditDataCleanUpBinding> TriggerAuditDataCleanUp()
        {
            var previousAcademicYearCollectionPeriod = _configuration.GetConfigurationValue(_environment, "PreviousAcademicYearCollectionPeriod");
            var previousAcademicYear = _configuration.GetConfigurationValue(_environment, "PreviousAcademicYear");
            var currentCollectionPeriod = _configuration.GetConfigurationValue(_environment, "CurrentCollectionPeriod");
            var currentAcademicYear = _configuration.GetConfigurationValue(_environment, "CurrentAcademicYear");

            var previousSubmissionJobsToBeDeletedBatches = await GetSubmissionJobsToBeDeletedBatches(previousAcademicYearCollectionPeriod, previousAcademicYear);
            var currentSubmissionJobsToBeDeletedBatches = await GetSubmissionJobsToBeDeletedBatches(currentCollectionPeriod, currentAcademicYear);

            var submissionJobsToBeDeletedBatches = previousSubmissionJobsToBeDeletedBatches.Union(currentSubmissionJobsToBeDeletedBatches).ToList();

            _logger.LogInformation($"Triggering Audit Data Cleanup for {submissionJobsToBeDeletedBatches.Count} submission job batches. " +
                                   $"DCJobIds: {string.Join(",", submissionJobsToBeDeletedBatches.SelectMany(x => x.JobsToBeDeleted.Select(y => y.DcJobId)))}");

            if (submissionJobsToBeDeletedBatches.Any())
            {
                return CreateAuditDataCleanUpBinding(submissionJobsToBeDeletedBatches);
            }

            return null;
        }

        private async Task<IEnumerable<SubmissionJobsToBeDeletedBatch>> GetSubmissionJobsToBeDeletedBatches(string collectionPeriod, string academicYear)
        {
            if (!string.IsNullOrWhiteSpace(collectionPeriod) && !string.IsNullOrWhiteSpace(academicYear))
            {
                return await _auditDataCleanUpDataservice.GetSubmissionJobsToBeDeletedBatches(collectionPeriod, academicYear);
            }

            return Enumerable.Empty<SubmissionJobsToBeDeletedBatch>();
        }

        private AuditDataCleanUpBinding CreateAuditDataCleanUpBinding(List<SubmissionJobsToBeDeletedBatch> batches)
        {
            var auditDataCleanUpBinding = new AuditDataCleanUpBinding();

            foreach (var batch in batches)
            {
                foreach (var job in batch.JobsToBeDeleted)
                {
                    var jobIdToBeDeleted = new List<SubmissionJobsToBeDeletedModel> { job }.ToArray();
                    auditDataCleanUpBinding.DataLock.Add(new DataLockAuditData { JobsToBeDeleted = jobIdToBeDeleted });
                    auditDataCleanUpBinding.EarningAudit.Add(new EarningAuditData { JobsToBeDeleted = jobIdToBeDeleted });
                    auditDataCleanUpBinding.FundingSource.Add(new FundingSourceAuditData { JobsToBeDeleted = jobIdToBeDeleted });
                    auditDataCleanUpBinding.RequiredPayments.Add(new RequiredPaymentAuditData { JobsToBeDeleted = jobIdToBeDeleted });
                }
            }

            return auditDataCleanUpBinding;
        }

        public async Task EarningEventAuditDataCleanUp(SubmissionJobsToBeDeletedBatch batch)
        {
            string earningAuditDataCleanUpQueue = _environment.IsDevelopment()
                ? _configuration.GetValue<string>("EarningAuditDataCleanUpQueue")
                : Environment.GetEnvironmentVariable("EarningAuditDataCleanUpQueue");

            await AuditDataCleanUp(DeleteEarningEventData, batch, earningAuditDataCleanUpQueue);
        }

        public async Task FundingSourceEventAuditDataCleanUp(SubmissionJobsToBeDeletedBatch batch)
        {
            string fundingSourceAuditDataCleanUpQueue = _environment.IsDevelopment()
                ? _configuration.GetValue<string>("FundingSourceAuditDataCleanUpQueue")
                : Environment.GetEnvironmentVariable("FundingSourceAuditDataCleanUpQueue");

            await AuditDataCleanUp(DeleteFundingSourceEvent, batch, fundingSourceAuditDataCleanUpQueue);
        }

        public async Task RequiredPaymentEventAuditDataCleanUp(SubmissionJobsToBeDeletedBatch batch)
        {
            string requiredPaymentAuditDataCleanUpQueue = _environment.IsDevelopment()
                ? _configuration.GetValue<string>("RequiredPaymentAuditDataCleanUpQueue")
                : Environment.GetEnvironmentVariable("RequiredPaymentAuditDataCleanUpQueue");

            await AuditDataCleanUp(DeleteRequiredPaymentEvent, batch, requiredPaymentAuditDataCleanUpQueue);
        }

        public async Task DataLockEventAuditDataCleanUp(SubmissionJobsToBeDeletedBatch batch)
        {
            string dataLockAuditDataCleanUpQueue = _environment.IsDevelopment()
                ? _configuration.GetValue<string>("DataLockAuditDataCleanUpQueue")
                : Environment.GetEnvironmentVariable("DataLockAuditDataCleanUpQueue");

            await AuditDataCleanUp(DeleteDataLockEvent, batch, dataLockAuditDataCleanUpQueue);
        }

        private async Task AuditDataCleanUp(Func<long, Task> deleteAuditData, SubmissionJobsToBeDeletedBatch batch, string queueName)
        {
            var deleteMethodName = deleteAuditData.Method.Name;

            try
            {
                _logger.LogInformation($"Started {deleteMethodName}");

                await deleteAuditData(batch.JobsToBeDeleted.First().DcJobId);

                _logger.LogInformation($"Finished {deleteMethodName}");
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Error Deleting Audit Data ({deleteMethodName}), internal Exception {e}");
                throw;
            }
        }

        private async Task DeleteEarningEventData(long jobId)
        {
            var earningEventPeriodCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $@"DELETE Payments2.EarningEventPeriod 
                       FROM Payments2.EarningEventPeriod AS EEP 
                           INNER JOIN Payments2.EarningEvent AS EE ON EEP.EarningEventId = EE.EventId 
                       WHERE EE.JobId = {jobId}", jobId);

            _logger.LogInformation($"DELETED {earningEventPeriodCount} earningEventPeriods for JobId {jobId}");

            var earningEventPriceEpisodeCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $@"DELETE Payments2.EarningEventPriceEpisode 
                       FROM Payments2.EarningEventPriceEpisode AS EEPE 
                          INNER JOIN Payments2.EarningEvent AS EE ON EEPE.EarningEventId = EE.EventId 
                       WHERE EE.JobId = {jobId}", jobId);

            _logger.LogInformation($"DELETED {earningEventPriceEpisodeCount} earningEventPriceEpisodes for JobId {jobId}");

            var earningEventCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $"DELETE Payments2.EarningEvent WHERE JobId = {jobId}", jobId);

            _logger.LogInformation($"DELETED {earningEventCount} EarningEvents for JobId {jobId}");
        }

        private async Task DeleteFundingSourceEvent(long jobId)
        {
            var fundingSourceEventCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $"DELETE Payments2.FundingSourceEvent WHERE JobId = {jobId} AND (Payments2.FundingSourceEvent.FundingPlatformType = 1 OR Payments2.FundingSourceEvent.FundingPlatformType IS NULL)",
                jobId);

            _logger.LogInformation($"DELETED {fundingSourceEventCount} FundingSourceEvents for JobId {jobId}");
        }

        private async Task DeleteRequiredPaymentEvent(long jobId)
        {
            var requiredPaymentEventCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $"DELETE Payments2.RequiredPaymentEvent WHERE JobId = {jobId}", jobId);

            _logger.LogInformation($"DELETED {requiredPaymentEventCount} RequiredPaymentEvents for JobId {jobId}");
        }

        private async Task DeleteDataLockEvent(long jobId)
        {
            var dataLockEventNonPayablePeriodFailuresCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $@"DELETE Payments2.DataLockEventNonPayablePeriodFailures 
                       FROM Payments2.DataLockEventNonPayablePeriodFailures AS DLENPPF 
                           INNER JOIN Payments2.DataLockEventNonPayablePeriod AS DLENPP ON DLENPPF.DataLockEventNonPayablePeriodId = DLENPP.DataLockEventNonPayablePeriodId 
                           INNER JOIN Payments2.DataLockEvent AS DL ON DLENPP.DataLockEventId = DL.EventId
                       WHERE DL.JobId = {jobId}", jobId);

            _logger.LogInformation($"DELETED {dataLockEventNonPayablePeriodFailuresCount} DataLockEventNonPayablePeriodFailures for JobId {jobId}");

            var dataLockEventNonPayablePeriodCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $@"DELETE Payments2.DataLockEventNonPayablePeriod 
                       FROM Payments2.DataLockEventNonPayablePeriod AS DLENPP
                           INNER JOIN Payments2.DataLockEvent AS DL ON DLENPP.DataLockEventId = DL.EventId
                       WHERE DL.JobId = {jobId}", jobId);

            _logger.LogInformation($"DELETED {dataLockEventNonPayablePeriodCount} DataLockEventNonPayablePeriods for JobId {jobId}");

            var dataLockEventPayablePeriodCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $@"DELETE Payments2.DataLockEventPayablePeriod 
                       FROM Payments2.DataLockEventPayablePeriod AS DLEPP
                           INNER JOIN Payments2.DataLockEvent AS DL ON DLEPP.DataLockEventId = DL.EventId
                       WHERE DL.JobId = {jobId}", jobId);

            _logger.LogInformation($"DELETED {dataLockEventPayablePeriodCount} DataLockEventPayablePeriods for JobId {jobId}");

            var dataLockEventPriceEpisodeCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $@"DELETE Payments2.DataLockEventPriceEpisode 
                       FROM Payments2.DataLockEventPriceEpisode AS DLEPP
                          INNER JOIN Payments2.DataLockEvent AS DL ON DLEPP.DataLockEventId = DL.EventId
                       WHERE DL.JobId = {jobId}", jobId);

            _logger.LogInformation($"DELETED {dataLockEventPriceEpisodeCount} DataLockEventPriceEpisodes for JobId {jobId}");

            var dataLockEventCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $"DELETE Payments2.DataLockEvent WHERE JobId = {jobId}", jobId);

            _logger.LogInformation($"DELETED {dataLockEventCount} DataLockEvents for JobId {jobId}");
        }

    }
}
