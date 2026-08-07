using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Model.Core.Entities;
using UUIDNext;

namespace SFA.DAS.Payments.ScheduledJobs.Tests.Specs.StepDefinitions
{
    [Binding]
    public class GsoAuditDataCleanUpStepDefinitions
    {
        private readonly TestSession _testSession;
        private CollectionPeriod _collectionPeriod;
        private long _ukprn;
        private string _learnerReferenceNumber;
        private string _courseCode;
        private HttpResponseMessage _functionAppResponse;

        private Guid _mostRecentExternalEarningsId;
        private readonly List<Guid> _supersededExternalEarningsIds = new();

        public GsoAuditDataCleanUpStepDefinitions(TestSession testSession)
        {
            this._testSession = testSession;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            _ukprn = 10000000 + _testSession.GenerateId(9999999);
            _learnerReferenceNumber = Guid.NewGuid().ToString("N").Substring(0, 10);
            _courseCode = Guid.NewGuid().ToString("N").Substring(0, 8);
            _collectionPeriod = new CollectionPeriodBuilder().WithDate(DateTime.Today).Build();
        }

        [AfterScenario]
        public async Task AfterScenario()
        {
            await DeleteExistingTestData();
        }

        [Given("a GSO short course payment has been recorded for a learner and course")]
        public async Task GivenAGsoShortCoursePaymentHasBeenRecordedForALearnerAndCourse()
        {
            _mostRecentExternalEarningsId = await CreateGsoPayment();
        }

        [Given("a more recent GSO short course payment has been recorded for the same learner and course")]
        public async Task GivenAMoreRecentGsoShortCoursePaymentHasBeenRecordedForTheSameLearnerAndCourse()
        {
            _supersededExternalEarningsIds.Add(_mostRecentExternalEarningsId);
            _mostRecentExternalEarningsId = await CreateGsoPayment();
        }

        [Given(@"the learner and course has (\d+) superseded GSO short course payments")]
        public async Task GivenTheLearnerAndCourseHasSupersededGsoShortCoursePayments(int count)
        {
            for (var i = 0; i < count; i++)
            {
                _supersededExternalEarningsIds.Add(_mostRecentExternalEarningsId);
                _mostRecentExternalEarningsId = await CreateGsoPayment();
            }
        }

        [When("the GSO audit data cleanup job is triggered")]
        public async Task WhenTheGsoAuditDataCleanUpJobIsTriggered()
        {
            var functionClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:7004")
            };

            _functionAppResponse = await functionClient.GetAsync("/api/GSOAuditDataCleanUpHttpTrigger");

            Assert.IsTrue(_functionAppResponse.IsSuccessStatusCode);
        }

        [Then("the audit data related to the most recent GSO payment for that learner and course is retained")]
        public async Task ThenTheAuditDataRelatedToTheMostRecentGsoPaymentForThatLearnerAndCourseIsRetained()
        {
            _testSession.DataContext.ChangeTracker.Clear();

            await _testSession.WaitForIt(() => _testSession.DataContext.RequiredPaymentEvents
                    .Any(x => x.ExternalEarningsId == _mostRecentExternalEarningsId && x.Ukprn == _ukprn),
                $"Most recent GSO payment not retained for ExternalEarningsId {_mostRecentExternalEarningsId}");
        }

        [Then("the audit data related to the superseded GSO payment is deleted")]
        [Then("the audit data related to the superseded GSO payments is deleted")]
        public async Task ThenTheAuditDataRelatedToTheSupersededGsoPaymentsIsDeleted()
        {
            _testSession.DataContext.ChangeTracker.Clear();

            foreach (var externalEarningsId in _supersededExternalEarningsIds)
            {
                await _testSession.WaitForItAndFail(() => _testSession.DataContext.RequiredPaymentEvents
                        .Any(x => x.ExternalEarningsId == externalEarningsId && x.Ukprn == _ukprn),
                    $"Superseded GSO payment is retained for ExternalEarningsId {externalEarningsId}");
            }
        }

        private async Task<Guid> CreateGsoPayment()
        {
            var externalEarningsId = Uuid.NewDatabaseFriendly(Database.SqlServer);

            var requiredPaymentEvent = new RequiredPaymentEventModel
            {
                EventId = Guid.NewGuid(),
                EarningEventId = Guid.NewGuid(),
                ExternalEarningsId = externalEarningsId,
                PriceEpisodeIdentifier = Guid.NewGuid().ToString(),
                Ukprn = _ukprn,
                ContractType = ContractType.Act1,
                TransactionType = TransactionType.Learning,
                SfaContributionPercentage = 1,
                Amount = 100,
                CollectionPeriod = new CollectionPeriod { AcademicYear = _collectionPeriod.AcademicYear, Period = _collectionPeriod.Period },
                DeliveryPeriod = 1,
                LearnerReferenceNumber = _learnerReferenceNumber,
                LearnerUln = 1000000000,
                LearningAimReference = "Learning",
                LearningAimProgrammeType = 1,
                LearningAimStandardCode = 1,
                LearningAimFrameworkCode = 1,
                LearningAimPathwayCode = 1,
                LearningAimFundingLineType = "FundingLine",
                CourseCode = _courseCode,
                CourseType = CourseType.ShortCourse,
                IlrSubmissionDateTime = DateTime.UtcNow,
                // JobId is always 0 for CourseType.ShortCourse in production - ExternalEarningsId is
                // the real identifier for GSO records, so test data mirrors that here.
                JobId = 0,
                EventTime = DateTimeOffset.UtcNow,
                StartDate = DateTime.UtcNow,
                CompletionStatus = 1,
                NumberOfInstalments = 1,
            };

            _testSession.DataContext.RequiredPaymentEvents.Add(requiredPaymentEvent);
            await _testSession.DataContext.SaveChangesAsync();

            await Task.Delay(5);

            return externalEarningsId;
        }

        private async Task DeleteExistingTestData()
        {
            const string sql = "DELETE FROM Payments2.RequiredPaymentEvent WHERE UKPRN = @ukprn AND LearnerReferenceNumber = @learnerReferenceNumber AND CourseCode = @courseCode;";

            var ukprnParameter = new SqlParameter("@ukprn", _ukprn);
            var learnerReferenceNumberParameter = new SqlParameter("@learnerReferenceNumber", _learnerReferenceNumber);
            var courseCodeParameter = new SqlParameter("@courseCode", _courseCode);

            await _testSession.DataContext.Database.ExecuteSqlRawAsync(sql, ukprnParameter, learnerReferenceNumberParameter, courseCodeParameter);

            _testSession.DataContext.ChangeTracker.Clear();
        }
    }
}
