using Bogus;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.ScheduledJobs.Tests.Specs.Data;
using System.Net;

namespace SFA.DAS.Payments.ScheduledJobs.Tests.Specs.StepDefinitions
{
    [Binding]
    public class StepDefinitions
    {
        private readonly ScenarioContext scenarioContext;
        private readonly MessagingContext messagingContext;
        private readonly TestSession testSession;
        private CollectionPeriod collectionPeriod;
        private long previousJobId;
        private long currentJobId;
        private long ukprn;
        private HttpResponseMessage functionAppResponse;

        public StepDefinitions(ScenarioContext scenarioContext, MessagingContext messagingContext, TestSession testSession)
        {
            this.scenarioContext = scenarioContext;
            this.messagingContext = messagingContext;
            this.testSession = testSession;
        }

        protected void SetCurrentCollectionYear()
        {
            collectionPeriod = new CollectionPeriodBuilder().WithDate(DateTime.Today).Build();
        }

        [BeforeScenario]
        public async Task BeforeScenario()
        {
            await DeleteExistingTestData();
            SetCurrentCollectionYear();
            previousJobId = 11111;
            currentJobId = 22222;
            ukprn = 10001234;
        }

        [AfterScenario]
        public void AfterScenario()
        {
        }

        [Given("the training provider has submitted their learners to be paid for")]
        public async Task GivenTheTrainingProviderHasSubmittedTheirLearnersToBePaidFor()
        {
            await CreateAuditData(currentJobId, DateTime.UtcNow.AddHours(-1));
        }

        [When("the audit data cleanup job is triggered")]
        public async Task WhenTheAuditDataCleanUpJobIsTriggered()
        {
            var functionClient = new HttpClient
            {
                BaseAddress = new Uri($"http://localhost:7004")
            };

            functionAppResponse = await functionClient.GetAsync("/api/TriggerAuditDataCleanUp");
            
            // Will return HTTP status code 400 / BadRequest if no work to do
            Assert.IsTrue(functionAppResponse.IsSuccessStatusCode || functionAppResponse.StatusCode == HttpStatusCode.BadRequest);
        }

        [Then("the audit data related to the current submission for that training provider is retained")]
        public async Task ThenTheAuditDataRelatedToTheCurrentSubmissionForThatTrainingProviderIsRetained()
        {
            testSession.DataContext.ChangeTracker.Clear();

            await testSession.WaitForIt(() => testSession.DataContext.DataLockEvents
                    .Any(x => x.JobId == currentJobId && x.Ukprn == ukprn),
                $"Data lock events not retained for job id {currentJobId}");

            await testSession.WaitForIt(() => testSession.DataContext.EarningEvents
                    .Any(x => x.JobId == currentJobId && x.Ukprn == ukprn),
                $"Earning events not retained for job id {currentJobId}");

            await testSession.WaitForIt(() => testSession.DataContext.FundingSourceEvents
                    .Any(x => x.JobId == currentJobId && x.Ukprn == ukprn),
                $"Funding source events not retained for job id {currentJobId}");

            await testSession.WaitForIt(() => testSession.DataContext.RequiredPaymentEvents
                    .Any(x => x.JobId == currentJobId && x.Ukprn == ukprn),
                $"Required payment events not retained for job id {currentJobId}");
        }

        [Given("the training provider has previously submitted their learners in the current collection period")]
        public async Task GivenTheTrainingProviderHasPreviouslySubmittedTheirLearnersInTheCurrentCollectionPeriod()
        {
            await CreateAuditData(previousJobId, DateTime.UtcNow.AddDays(-1));
        }

        [Then("the audit data related to the previous submission is deleted")]
        public async Task ThenTheAuditDataRelatedToThePreviousSubmissionIsDeleted()
        {
            testSession.DataContext.ChangeTracker.Clear();

            await testSession.WaitForItAndFail(() => testSession.DataContext.DataLockEvents
                    .Any(x => x.JobId == previousJobId && x.Ukprn == ukprn),
                $"Data lock events are retained for job id {previousJobId}");

            await testSession.WaitForItAndFail(() => testSession.DataContext.EarningEvents
                    .Any(x => x.JobId == previousJobId && x.Ukprn == ukprn),
                $"Earning events are retained for job id {previousJobId}");

            await testSession.WaitForItAndFail(() => testSession.DataContext.FundingSourceEvents
                    .Any(x => x.JobId == previousJobId && x.Ukprn == ukprn),
                $"Funding source events are retained for job id {previousJobId}");

            await testSession.WaitForItAndFail(() => testSession.DataContext.RequiredPaymentEvents
                    .Any(x => x.JobId == previousJobId && x.Ukprn == ukprn),
                $"Required payment events are retained for job id {previousJobId}");
        }
        
        private async Task CreateAuditData(long jobId, DateTime ilrSubmisssionDateTime)
        {
            var dataLockEventFaker = new Faker<DataLockEventModel>()
                .RuleFor(x => x.Id, 0)
                .RuleFor(x => x.EventId, f => Guid.NewGuid())
                .RuleFor(x => x.EarningEventId, f => Guid.NewGuid())
                .RuleFor(x => x.Ukprn, ukprn)
                .RuleFor(x => x.ContractType, ContractType.Act1)
                .RuleFor(x => x.CollectionPeriod, collectionPeriod.Period)
                .RuleFor(x => x.AcademicYear, collectionPeriod.AcademicYear)
                .RuleFor(x => x.LearnerReferenceNumber, f => f.Random.AlphaNumeric(10))
                .RuleFor(x => x.LearnerUln, f => f.Random.Long(1000000000, 9999999999))
                .RuleFor(x => x.LearningAimReference, f => f.Random.String2(8, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"))
                .RuleFor(x => x.LearningAimProgrammeType, f => f.Random.Int(0, 99))
                .RuleFor(x => x.LearningAimStandardCode, f => f.Random.Int(0, 9999))
                .RuleFor(x => x.LearningAimFrameworkCode, f => f.Random.Int(0, 999))
                .RuleFor(x => x.LearningAimPathwayCode, f => f.Random.Int(0, 99))
                .RuleFor(x => x.IlrSubmissionDateTime, ilrSubmisssionDateTime)
                .RuleFor(x => x.IsPayable, f => f.Random.Bool())
                .RuleFor(x => x.DataLockSource, DataLockSource.Submission)
                .RuleFor(x => x.JobId, jobId)
                .RuleFor(x => x.EventTime, f => f.Date.RecentOffset())
            ;

            var dataLockEvent = dataLockEventFaker.Generate(1).First();

            var dataLockEventPriceEpisodesFaker = new Faker<DataLockEventPriceEpisodeModel>()
                .RuleFor(x => x.DataLockEventId, _ => dataLockEvent.EventId)
                .RuleFor(x => x.PriceEpisodeIdentifier, f => $"PE-{f.Random.AlphaNumeric(12).ToUpperInvariant()}")
                .RuleFor(x => x.SfaContributionPercentage, f => f.Finance.Amount(0m, 1m, 2))
                .RuleFor(x => x.TotalNegotiatedPrice1, f => f.Finance.Amount(1_000m, 15_000m, 2))
                .RuleFor(x => x.TotalNegotiatedPrice2, f => f.Finance.Amount(0m, 5_000m, 2))
                .RuleFor(x => x.TotalNegotiatedPrice3, f => f.Finance.Amount(0m, 5_000m, 2))
                .RuleFor(x => x.TotalNegotiatedPrice4, f => f.Finance.Amount(0m, 5_000m, 2))
                .RuleFor(x => x.StartDate, f => f.Date.Past(1).Date)
                .RuleFor(x => x.PlannedEndDate, (f, x) => x.StartDate.AddMonths(f.Random.Int(12, 48)))
                .RuleFor(x => x.NumberOfInstalments, f => f.Random.Int(1, 36))
                .RuleFor(x => x.InstalmentAmount, f => f.Finance.Amount(100m, 1_500m, 2))
                .RuleFor(x => x.CompletionAmount, f => f.Finance.Amount(0m, 3_000m, 2))
                .RuleFor(x => x.Completed, false)
                .RuleFor(x => x.EffectiveTotalNegotiatedPriceStartDate, (_, x) => x.StartDate)
                .RuleFor(x => x.AcademicYear, collectionPeriod.AcademicYear)
                .RuleFor(x => x.CollectionPeriod, collectionPeriod.Period); 
            ;

            var dataLockEventPayablePeriodsFaker = new Faker<DataLockEventPayablePeriodModel>()
                .RuleFor(x => x.DataLockEventId, _ => dataLockEvent.EventId)
                .RuleFor(x => x.PriceEpisodeIdentifier, f => $"PE-{f.Random.AlphaNumeric(12).ToUpperInvariant()}")
                .RuleFor(x => x.TransactionType, TransactionType.Learning)
                .RuleFor(x => x.AcademicYear, collectionPeriod.AcademicYear)
                .RuleFor(x => x.CollectionPeriod, collectionPeriod.Period)
                .RuleFor(x => x.DeliveryPeriod, f => f.Random.Byte(1, 12))
                .RuleFor(x => x.Amount, f => f.Finance.Amount(1m, 5000m, 2))
                .RuleFor(x => x.SfaContributionPercentage,  f => f.Finance.Amount(0m, 1m, 2))
                .RuleFor(x => x.LearningStartDate, f =>  f.Date.Past(2).Date)
                .RuleFor(x => x.ApprenticeshipId, f => f.Random.Long(100000, 9999999))
                .RuleFor(x => x.ApprenticeshipPriceEpisodeId, f =>  f.Random.Long(100000, 9999999))
                .RuleFor(x => x.ApprenticeshipEmployerType, ApprenticeshipEmployerType.Levy);
            ;

            var dataLockEventNonPayablePeriodsFaker = new Faker<DataLockEventNonPayablePeriodModel>()
                .RuleFor(x => x.DataLockEventId, dataLockEvent.EventId)
                .RuleFor(x => x.DataLockEventNonPayablePeriodId, Guid.NewGuid())
                .RuleFor(x => x.PriceEpisodeIdentifier, f => $"PE-{f.Random.AlphaNumeric(12).ToUpperInvariant()}")
                .RuleFor(x => x.TransactionType, TransactionType.Learning)
                .RuleFor(x => x.AcademicYear, collectionPeriod.AcademicYear)
                .RuleFor(x => x.CollectionPeriod, collectionPeriod.Period)
                .RuleFor(x => x.DeliveryPeriod, f => f.Random.Byte(1, 12))
                .RuleFor(x => x.Amount, f => f.Finance.Amount(1m, 5000m, 2))
                .RuleFor(x => x.SfaContributionPercentage, f => f.Finance.Amount(0m, 1m, 2))
                .RuleFor(x => x.LearningStartDate, f => f.Date.Past(2).Date)
            ;

            dataLockEvent.PriceEpisodes = [dataLockEventPriceEpisodesFaker.Generate(1).First()];
            dataLockEvent.PayablePeriods = [dataLockEventPayablePeriodsFaker.Generate(1).First()];
            dataLockEvent.NonPayablePeriods = [dataLockEventNonPayablePeriodsFaker.Generate(1).First()];

            var earningEventFaker = new Faker<EarningEventModel>()
                .RuleFor(x => x.EventId, f => Guid.NewGuid())
                .RuleFor(x => x.Ukprn, ukprn)
                .RuleFor(x => x.ContractType, ContractType.Act1)
                .RuleFor(x => x.CollectionPeriod, collectionPeriod.Period)
                .RuleFor(x => x.AcademicYear, collectionPeriod.AcademicYear)
                .RuleFor(x => x.LearnerReferenceNumber, f => f.Random.AlphaNumeric(10))
                .RuleFor(x => x.LearnerUln, f => f.Random.Long(1000000000, 9999999999))
                .RuleFor(x => x.LearningAimReference, f => f.Random.String2(8, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"))
                .RuleFor(x => x.LearningAimProgrammeType, f => f.Random.Int(0, 99))
                .RuleFor(x => x.LearningAimStandardCode, f => f.Random.Int(0, 9999))
                .RuleFor(x => x.LearningAimFrameworkCode, f => f.Random.Int(0, 999))
                .RuleFor(x => x.LearningAimPathwayCode, f => f.Random.Int(0, 99))
                .RuleFor(x => x.IlrSubmissionDateTime, ilrSubmisssionDateTime)
                .RuleFor(x => x.JobId, jobId)
                .RuleFor(x => x.EventTime, f => f.Date.RecentOffset())
            ;

            var earningEvent = earningEventFaker.Generate(1).First();

            var earningEventPeriodFaker = new Faker<EarningEventPeriodModel>()
                .RuleFor(x => x.EarningEventId, _ => earningEvent.EventId)
                .RuleFor(x => x.PriceEpisodeIdentifier, f => $"PE-{f.Random.AlphaNumeric(12).ToUpperInvariant()}")
                .RuleFor(x => x.TransactionType, _ => TransactionType.Learning)
                .RuleFor(x => x.AcademicYear, _ => collectionPeriod.AcademicYear)
                .RuleFor(x => x.CollectionPeriod, _ => collectionPeriod.Period)
                .RuleFor(x => x.DeliveryPeriod, f => f.Random.Byte(1, 12))
                .RuleFor(x => x.Amount, f => f.Finance.Amount(1m, 5000m, 2))
                .RuleFor(x => x.SfaContributionPercentage, f => f.Finance.Amount(0m, 1m, 2))
                .RuleFor(x => x.CensusDate, _ => DateTime.Today);

            var earningEventPriceEpisodeFaker = new Faker<EarningEventPriceEpisodeModel>()
                .RuleFor(x => x.EarningEventId, _ => earningEvent.EventId)
                .RuleFor(x => x.PriceEpisodeIdentifier, f => $"PE-{f.Random.AlphaNumeric(12).ToUpperInvariant()}")
                .RuleFor(x => x.SfaContributionPercentage, f => f.Finance.Amount(0m, 100m, 2))
                .RuleFor(x => x.TotalNegotiatedPrice1, f => f.Finance.Amount(1000m, 15000m, 2))
                .RuleFor(x => x.TotalNegotiatedPrice2, f => f.Finance.Amount(0m, 5000m, 2))
                .RuleFor(x => x.TotalNegotiatedPrice3, f => f.Finance.Amount(0m, 5000m, 2))
                .RuleFor(x => x.TotalNegotiatedPrice4, f => f.Finance.Amount(0m, 5000m, 2))
                .RuleFor(x => x.StartDate, f => f.Date.Past(1).Date)
                .RuleFor(x => x.PlannedEndDate, (f, x) => x.StartDate.AddMonths(f.Random.Int(12, 48)))
                .RuleFor(x => x.Completed, f => f.Random.Bool())
                .RuleFor(x => x.ActualEndDate, (f, x) => x.Completed ? f.Date.Between(x.StartDate, x.PlannedEndDate).Date : null)
                .RuleFor(x => x.NumberOfInstalments, f => f.Random.Int(1, 36))
                .RuleFor(x => x.InstalmentAmount, f => f.Finance.Amount(0m, 5000m, 2))
                .RuleFor(x => x.CompletionAmount, f => f.Finance.Amount(0m, 5000m, 2))
                .RuleFor(x => x.EffectiveTotalNegotiatedPriceStartDate, (f, x) => x.StartDate)
                .RuleFor(x => x.EmployerContribution, f => f.Finance.Amount(0m, 5000m, 2))
                .RuleFor(x => x.AgreedPrice, (f => f.Finance.Amount(0m, 5000m, 2)))
                .RuleFor(x => x.CourseStartDate, (f, x) => x.StartDate)
                .RuleFor(x => x.AcademicYear, collectionPeriod.AcademicYear)
                .RuleFor(x => x.CollectionPeriod, collectionPeriod.Period);

            earningEvent.Periods = [earningEventPeriodFaker.Generate(1).First()];
            earningEvent.PriceEpisodes = [earningEventPriceEpisodeFaker.Generate(1).First()];

            var fundingSourceEventFaker = new Faker<FundingSourceEventModel>()
                .RuleFor(x => x.EventId, _ => Guid.NewGuid())
                .RuleFor(x => x.EarningEventId, _ => Guid.NewGuid())
                .RuleFor(x => x.RequiredPaymentEventId, _ => Guid.NewGuid())
                .RuleFor(x => x.EventTime, f => f.Date.RecentOffset())
                .RuleFor(x => x.JobId, jobId)
                .RuleFor(x => x.DeliveryPeriod, f => f.Random.Byte(1, 12))
                .RuleFor(x => x.CollectionPeriod, new CollectionPeriod { AcademicYear = collectionPeriod.AcademicYear, Period = collectionPeriod.Period })
                .RuleFor(x => x.Ukprn, ukprn)
                .RuleFor(x => x.LearnerReferenceNumber, f => f.Random.AlphaNumeric(10))
                .RuleFor(x => x.LearnerUln, f => f.Random.Long(1000000000, 9999999999))
                .RuleFor(x => x.PriceEpisodeIdentifier, f => f.Random.Guid().ToString())
                .RuleFor(x => x.Amount, f => f.Finance.Amount(1, 5000))
                .RuleFor(x => x.LearningAimReference, f => f.Random.String2(8, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"))
                .RuleFor(x => x.LearningAimProgrammeType, f => f.Random.Int(0, 99))
                .RuleFor(x => x.LearningAimStandardCode, f => f.Random.Int(0, 9999))
                .RuleFor(x => x.LearningAimFrameworkCode, f => f.Random.Int(0, 999))
                .RuleFor(x => x.LearningAimPathwayCode, f => f.Random.Int(0, 99))
                .RuleFor(x => x.LearningAimFundingLineType, f => f.Commerce.Department())
                .RuleFor(x => x.ContractType, ContractType.Act1)
                .RuleFor(x => x.TransactionType, TransactionType.Learning)
                .RuleFor(x => x.FundingSource, FundingSourceType.Levy)
                .RuleFor(x => x.IlrSubmissionDateTime, ilrSubmisssionDateTime)
                .RuleFor(x => x.SfaContributionPercentage, f => f.Finance.Amount(0, 1))
                .RuleFor(x => x.StartDate, f => f.Date.Past())
                .RuleFor(x => x.CompletionStatus, f => f.Random.Byte(0, 2))
                .RuleFor(x => x.NumberOfInstalments, f => (short)f.Random.Int(1, 24))
                .RuleFor(x => x.FundingPlatformType, FundingPlatformType.SubmitLearnerData)
            ;
            var requiredPaymentEventFaker = new Faker<RequiredPaymentEventModel>()
                .RuleFor(x => x.EventId, _ => Guid.NewGuid())
                .RuleFor(x => x.EarningEventId, _ => Guid.NewGuid())
                .RuleFor(x => x.PriceEpisodeIdentifier, f => f.Random.Guid().ToString())
                .RuleFor(x => x.Ukprn, ukprn)
                .RuleFor(x => x.ContractType, ContractType.Act1)
                .RuleFor(x => x.TransactionType, TransactionType.Learning)
                .RuleFor(x => x.SfaContributionPercentage, f => f.Finance.Amount(0, 1))
                .RuleFor(x => x.Amount, f => f.Finance.Amount(1, 5000))
                .RuleFor(x => x.CollectionPeriod, new CollectionPeriod { AcademicYear = collectionPeriod.AcademicYear, Period = collectionPeriod.Period })
                .RuleFor(x => x.DeliveryPeriod, f => f.Random.Byte(1, 12))
                .RuleFor(x => x.LearnerReferenceNumber, f => f.Random.AlphaNumeric(10))
                .RuleFor(x => x.LearnerUln, f => f.Random.Long(1000000000, 9999999999))
                .RuleFor(x => x.LearningAimReference, f => f.Random.String2(8, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"))
                .RuleFor(x => x.LearningAimProgrammeType, f => f.Random.Int(0, 99))
                .RuleFor(x => x.LearningAimStandardCode, f => f.Random.Int(0, 9999))
                .RuleFor(x => x.LearningAimFrameworkCode, f => f.Random.Int(0, 999))
                .RuleFor(x => x.LearningAimPathwayCode, f => f.Random.Int(0, 99))
                .RuleFor(x => x.LearningAimFundingLineType, f => f.Commerce.Department())
                .RuleFor(x => x.IlrSubmissionDateTime, ilrSubmisssionDateTime)
                .RuleFor(x => x.JobId, _ => jobId)
                .RuleFor(x => x.EventTime, f => f.Date.RecentOffset())
                .RuleFor(x => x.StartDate, f => f.Date.Past())
                .RuleFor(x => x.CompletionStatus, f => f.Random.Byte(0, 2))
                .RuleFor(x => x.NumberOfInstalments, f => (short)f.Random.Int(1, 24));

            var submissionJobFaker = new Faker<JobModel>()
                .RuleFor(x => x.DcJobId, jobId)
                .RuleFor(x => x.Ukprn, ukprn)
                .RuleFor(x => x.DcJobSucceeded, true)
                .RuleFor(x => x.JobType, JobType.EarningsJob)
                .RuleFor(x => x.StartTime, f => f.Date.RecentOffset())
                .RuleFor(x => x.Status, JobStatus.Completed)
                .RuleFor(x => x.AcademicYear, collectionPeriod.AcademicYear)
                .RuleFor(x => x.CollectionPeriod, collectionPeriod.Period)
                .RuleFor(x => x.IlrSubmissionTime, ilrSubmisssionDateTime)
            ;
            
            testSession.DataContext.DataLockEvents.Add(dataLockEvent);
            testSession.DataContext.EarningEvents.Add(earningEventFaker.Generate(1).First());
            testSession.DataContext.FundingSourceEvents.Add(fundingSourceEventFaker.Generate(1).First());
            testSession.DataContext.RequiredPaymentEvents.Add(requiredPaymentEventFaker.Generate(1).First());
            testSession.DataContext.Jobs.Add(submissionJobFaker.Generate(1).First());

            await testSession.DataContext.SaveChangesAsync();
        }

        private async Task DeleteExistingTestData()
        {
            const string sql = """
                               DELETE FROM Payments2.FundingSourceEvent
                               WHERE UKPRN = @ukprn;

                               DELETE FROM Payments2.RequiredPaymentEvent
                               WHERE UKPRN = @ukprn;

                               DELETE FROM Payments2.DataLockEvent
                               WHERE UKPRN = @ukprn;

                               DELETE FROM Payments2.EarningEvent
                               WHERE UKPRN = @ukprn;

                               DELETE FROM Payments2.Job
                               WHERE UKPRN = @ukprn;
                               """;

            var ukprnParameter = new SqlParameter("@ukprn", ukprn);

            await testSession.DataContext.Database.ExecuteSqlRawAsync(sql, ukprnParameter);

            testSession.DataContext.ChangeTracker.Clear();
        }

    }
}
