using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Payments.Core;
using SFA.DAS.Payments.ScheduledJobs.Bindings;
using SFA.DAS.Payments.ScheduledJobs.Common;

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
            IEnumerable<SubmissionJobsToBeDeletedBatch> previousSubmissionJobsToBeDeletedBatches = new List<SubmissionJobsToBeDeletedBatch>();

            string previousAcademicYearCollectionPeriod = _environment.IsDevelopment()
                ? _configuration.GetValue<string>("PreviousAcademicYearCollectionPeriod")
                : Environment.GetEnvironmentVariable("PreviousAcademicYearCollectionPeriod");

            string previousAcademicYear = _environment.IsDevelopment()
                ? _configuration.GetValue<string>("PreviousAcademicYear")
                : Environment.GetEnvironmentVariable("PreviousAcademicYear");

            string currentCollectionPeriod = _environment.IsDevelopment()
                 ? _configuration.GetValue<string>("CurrentCollectionPeriod")
                 : Environment.GetEnvironmentVariable("CurrentCollectionPeriod");

            string currentAcademicYear = _environment.IsDevelopment()
                ? _configuration.GetValue<string>("CurrentAcademicYear")
                : Environment.GetEnvironmentVariable("CurrentAcademicYear");

            if (!string.IsNullOrWhiteSpace(previousAcademicYearCollectionPeriod) && !string.IsNullOrWhiteSpace(previousAcademicYear))
            {
                previousSubmissionJobsToBeDeletedBatches = await _auditDataCleanUpDataservice.GetSubmissionJobsToBeDeletedBatches(previousAcademicYearCollectionPeriod, previousAcademicYear);
            }

            var currentSubmissionJobsToBeDeletedBatches = await _auditDataCleanUpDataservice.GetSubmissionJobsToBeDeletedBatches(currentCollectionPeriod, currentAcademicYear);

            var submissionJobsToBeDeletedBatches = previousSubmissionJobsToBeDeletedBatches.Union(currentSubmissionJobsToBeDeletedBatches);
            var submissionJobsToBeDeletedBatchesList = submissionJobsToBeDeletedBatches.ToList();
            var batchProcessor = submissionJobsToBeDeletedBatchesList;

            _logger.LogInformation($"Triggering Audit Data Cleanup for {submissionJobsToBeDeletedBatchesList.Count} submission job batches. " +
                                  $"DCJobIds: {string.Join(",", submissionJobsToBeDeletedBatchesList.SelectMany(x => x.JobsToBeDeleted.Select(y => y.DcJobId)))}");

            if (batchProcessor.Count() > 0)
            {
                var AuditDataCleanUpBinding = new AuditDataCleanUpBinding();
                
                foreach (var batch in batchProcessor)
                {
                    var dataLockAuditData = new DataLockAuditData
                    {
                        JobsToBeDeleted = batch.JobsToBeDeleted
                    };
                    var EarningAuditData = new EarningAuditData
                    {
                        JobsToBeDeleted = batch.JobsToBeDeleted
                    };
                    var FundingSourceAuditData = new FundingSourceAuditData
                    {
                        JobsToBeDeleted = batch.JobsToBeDeleted
                    };
                    var RequiredPaymentAuditData = new RequiredPaymentAuditData
                    {
                        JobsToBeDeleted = batch.JobsToBeDeleted
                    };

                    AuditDataCleanUpBinding.DataLock.Add(dataLockAuditData);
                    AuditDataCleanUpBinding.EarningAudit.Add(EarningAuditData);
                    AuditDataCleanUpBinding.FundingSource.Add(FundingSourceAuditData);
                    AuditDataCleanUpBinding.RequiredPayments.Add(RequiredPaymentAuditData);
                }

                return AuditDataCleanUpBinding;
            }
            return null;

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

        private async Task SplitBatchAndEnqueueMessages(SubmissionJobsToBeDeletedBatch batch, string queueName)
        {
            foreach (var jobsToBeDeletedModel in batch.JobsToBeDeleted)
            {
                var newSplittedBatchitem = new SubmissionJobsToBeDeletedBatch { JobsToBeDeleted = new[] { jobsToBeDeletedModel } };
                var serializedMessage = JsonConvert.SerializeObject(newSplittedBatchitem);
                await _serviceBusClientHelper.SendMessageToQueueAsync(queueName, serializedMessage);
            }
        }

        private async Task AuditDataCleanUp(Func<IList<SqlParameter>, string, string, Task> deleteAuditData, SubmissionJobsToBeDeletedBatch batch, string queueName)
        {
            try
            {
                var sqlParameters = batch.JobsToBeDeleted.ToSqlParameters();

                var deleteMethodName = deleteAuditData.Method.Name;

                _logger.LogInformation($"Started {deleteMethodName}");

                var sqlParamName = string.Join(", ", sqlParameters.Select(pn => pn.ParameterName));
                var paramValues = string.Join(", ", sqlParameters.Select(pn => pn.Value));

                await deleteAuditData((IList<SqlParameter>)sqlParameters, sqlParamName, paramValues);

                _logger.LogInformation($"Finished {deleteMethodName}");
            }
            catch (Exception e)
            {
                //we have already tried in single batch mode nothing more can be done here
                if (batch.JobsToBeDeleted.Length == 1)
                {
                    _logger.LogWarning($"Error Deleting Audit Data, internal Exception {e}");
                    throw;
                }

                //if SQL TimeOut or Dead-lock and we haven't already tried with single item Mode then try again with Batch Split into single items
                if (e.IsTimeOutException() || e.IsDeadLockException())
                {
                    _logger.LogWarning($"Starting Audit Data Deletion in Single Item mode");

                    await SplitBatchAndEnqueueMessages(batch, queueName);
                }
            }
        }

        private async Task DeleteEarningEventData(IList<SqlParameter> sqlParameters, string sqlParamName, string paramValues)
        {
            var earningEventPeriodCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $@"DELETE Payments2.EarningEventPeriod 
                       FROM Payments2.EarningEventPeriod AS EEP 
                           INNER JOIN Payments2.EarningEvent AS EE ON EEP.EarningEventId = EE.EventId 
                       WHERE EE.JobId IN ({sqlParamName})",
                sqlParameters);

            _logger.LogInformation($"DELETED {earningEventPeriodCount} earningEventPeriods for JobIds {paramValues}");

            var earningEventPriceEpisodeCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $@"DELETE Payments2.EarningEventPriceEpisode 
                       FROM Payments2.EarningEventPriceEpisode AS EEPE 
                          INNER JOIN Payments2.EarningEvent AS EE ON EEPE.EarningEventId = EE.EventId 
                       WHERE EE.JobId IN ({sqlParamName})",
                sqlParameters);

            _logger.LogInformation($"DELETED {earningEventPriceEpisodeCount} earningEventPriceEpisodes for JobIds {paramValues}");

            var earningEventCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $"DELETE Payments2.EarningEvent WHERE JobId IN ({sqlParamName})",
                sqlParameters);

            _logger.LogInformation($"DELETED {earningEventCount} EarningEvents for JobIds {paramValues}");
        }

        private async Task DeleteFundingSourceEvent(IList<SqlParameter> sqlParameters, string sqlParamName, string paramValues)
        {
            var fundingSourceEventCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $"DELETE Payments2.FundingSourceEvent WHERE JobId IN ({sqlParamName})",
                sqlParameters);

            _logger.LogInformation($"DELETED {fundingSourceEventCount} FundingSourceEvents for JobIds {paramValues}");
        }

        private async Task DeleteRequiredPaymentEvent(IList<SqlParameter> sqlParameters, string sqlParamName, string paramValues)
        {
            var requiredPaymentEventCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $"DELETE Payments2.RequiredPaymentEvent WHERE JobId IN ({sqlParamName})",
                sqlParameters);

            _logger.LogInformation($"DELETED {requiredPaymentEventCount} RequiredPaymentEvents for JobIds {paramValues}");
        }

        private async Task DeleteDataLockEvent(IList<SqlParameter> sqlParameters, string sqlParamName, string paramValues)
        {
            var dataLockEventNonPayablePeriodFailuresCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $@"DELETE Payments2.DataLockEventNonPayablePeriodFailures 
                       FROM Payments2.DataLockEventNonPayablePeriodFailures AS DLENPPF 
                           INNER JOIN Payments2.DataLockEventNonPayablePeriod AS DLENPP ON DLENPPF.DataLockEventNonPayablePeriodId = DLENPP.DataLockEventNonPayablePeriodId 
                           INNER JOIN Payments2.DataLockEvent AS DL ON DLENPP.DataLockEventId = DL.EventId
                       WHERE DL.JobId IN ({sqlParamName})",
                sqlParameters);

            _logger.LogInformation($"DELETED {dataLockEventNonPayablePeriodFailuresCount} DataLockEventNonPayablePeriodFailures for JobIds {paramValues}");

            var dataLockEventNonPayablePeriodCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $@"DELETE Payments2.DataLockEventNonPayablePeriod 
                       FROM Payments2.DataLockEventNonPayablePeriod AS DLENPP
                           INNER JOIN Payments2.DataLockEvent AS DL ON DLENPP.DataLockEventId = DL.EventId
                       WHERE DL.JobId IN ({sqlParamName})",
                sqlParameters);

            _logger.LogInformation($"DELETED {dataLockEventNonPayablePeriodCount} DataLockEventNonPayablePeriods for JobIds {paramValues}");

            var dataLockEventPayablePeriodCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $@"DELETE Payments2.DataLockEventPayablePeriod 
                       FROM Payments2.DataLockEventPayablePeriod AS DLEPP
                           INNER JOIN Payments2.DataLockEvent AS DL ON DLEPP.DataLockEventId = DL.EventId
                       WHERE DL.JobId IN ({sqlParamName})",
                sqlParameters);

            _logger.LogInformation($"DELETED {dataLockEventPayablePeriodCount} DataLockEventPayablePeriods for JobIds {paramValues}");

            var dataLockEventPriceEpisodeCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $@"DELETE Payments2.DataLockEventPriceEpisode 
                       FROM Payments2.DataLockEventPriceEpisode AS DLEPP
                          INNER JOIN Payments2.DataLockEvent AS DL ON DLEPP.DataLockEventId = DL.EventId
                       WHERE DL.JobId IN ({sqlParamName})",
                sqlParameters);

            _logger.LogInformation($"DELETED {dataLockEventPriceEpisodeCount} DataLockEventPriceEpisodes for JobIds {paramValues}");

            var dataLockEventCount = await _paymentDataContext.Database.ExecuteSqlRawAsync(
                $"DELETE Payments2.DataLockEvent WHERE JobId IN ({sqlParamName})",
                sqlParameters);

            _logger.LogInformation($"DELETED {dataLockEventCount} DataLockEvents for JobIds {paramValues}");
        }

    }
}
