using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.ScheduledJobs.ServiceBus;

namespace SFA.DAS.Payments.ScheduledJobs.UnitTests.Services
{
    [TestFixture]
    public class AuditDataCleanUpServiceTests
    {
        private Mock<ILogger<AuditDataCleanUpService>> _mockLogger;
        private Mock<IServiceBusClientHelper> _mockServiceBusClientHelper;
        private IPaymentsDataContext _paymentsDataContext;
        private Mock<IAuditDataCleanUpDataservice> _auditDataCleanUpDataserviceMock;

        private Mock<IConfiguration> _configuration;
        private Mock<IHostEnvironment> _environment;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger<AuditDataCleanUpService>>();
            _mockServiceBusClientHelper = new Mock<IServiceBusClientHelper>();
            _auditDataCleanUpDataserviceMock = new Mock<IAuditDataCleanUpDataservice>();
            _configuration = new Mock<IConfiguration>();
            _environment = new Mock<IHostEnvironment>(MockBehavior.Strict);

            var options = new DbContextOptionsBuilder<PaymentsDataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _paymentsDataContext = new PaymentsDataContext(options);

            _environment.SetupGet(x => x.EnvironmentName).Returns(Environments.Development);
        }

        private AuditDataCleanUpService CreateSut() =>
            new AuditDataCleanUpService(
                _paymentsDataContext,
                _mockLogger.Object,
                _mockServiceBusClientHelper.Object,
                _auditDataCleanUpDataserviceMock.Object,
                _configuration.Object,
                _environment.Object);

        private static SubmissionJobsToBeDeletedBatch Batch(params int[] dcJobIds) =>
            new SubmissionJobsToBeDeletedBatch
            {
                JobsToBeDeleted = dcJobIds.Select(id => new SubmissionJobsToBeDeletedModel { DcJobId = id }).ToArray()
            };

        private void SetupConfigValue(string key, string value)
        {
            var section = new Mock<IConfigurationSection>();
            section.SetupGet(s => s.Value).Returns(value);
            _configuration.Setup(c => c.GetSection(key)).Returns(section.Object);
        }

        private void SetDevConfigPeriods(
            string prevPeriod = "07", string prevAy = "2324",
            string curPeriod = "08", string curAy = "2324")
        {
            _environment.SetupGet(x => x.EnvironmentName).Returns(Environments.Development);
            SetupConfigValue("PreviousAcademicYearCollectionPeriod", prevPeriod);
            SetupConfigValue("PreviousAcademicYear", prevAy);
            SetupConfigValue("CurrentCollectionPeriod", curPeriod);
            SetupConfigValue("CurrentAcademicYear", curAy);
        }

        private void SetNonDevEnvPeriods(
            string prevPeriod = "07", string prevAy = "2324",
            string curPeriod = "08", string curAy = "2324")
        {
            _environment.SetupGet(x => x.EnvironmentName).Returns(Environments.Production);

            Environment.SetEnvironmentVariable("PreviousAcademicYearCollectionPeriod", prevPeriod);
            Environment.SetEnvironmentVariable("PreviousAcademicYear", prevAy);
            Environment.SetEnvironmentVariable("CurrentCollectionPeriod", curPeriod);
            Environment.SetEnvironmentVariable("CurrentAcademicYear", curAy);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("PreviousAcademicYearCollectionPeriod", null);
            Environment.SetEnvironmentVariable("PreviousAcademicYear", null);
            Environment.SetEnvironmentVariable("CurrentCollectionPeriod", null);
            Environment.SetEnvironmentVariable("CurrentAcademicYear", null);
        }

        [Test]
        public async Task TriggerAuditDataCleanUp_ShouldLogInformation()
        {
            // Arrange
            SetDevConfigPeriods(curPeriod: "2021", curAy: "2021/22");

            var jobjsToBeDeleted = new List<SubmissionJobsToBeDeletedBatch>
            {
                new SubmissionJobsToBeDeletedBatch
                {
                    JobsToBeDeleted = new[] { new SubmissionJobsToBeDeletedModel { DcJobId = 1 } }
                }
            };

            _auditDataCleanUpDataserviceMock
                .Setup(a => a.GetSubmissionJobsToBeDeletedBatches(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(jobjsToBeDeleted);

            var service = new AuditDataCleanUpService(_paymentsDataContext,
                _mockLogger.Object,
                _mockServiceBusClientHelper.Object,
                _auditDataCleanUpDataserviceMock.Object,
                _configuration.Object,
                _environment.Object);

            // Act
            var result = await service.TriggerAuditDataCleanUp();

            // Assert
            result.FundingSource.Should().NotBeNull();
            result.FundingSource.Should().BeOfType<List<FundingSourceAuditData>>();
            result.FundingSource.First().JobsToBeDeleted.Should().NotBeNull();
            result.FundingSource.First().JobsToBeDeleted.Should().HaveCount(jobjsToBeDeleted.Count);
            result.FundingSource.First().JobsToBeDeleted.First().DcJobId.Should().Be(1);

            result.DataLock.Should().NotBeNull();
            result.DataLock.Should().BeOfType<List<DataLockAuditData>>();
            result.DataLock.First().JobsToBeDeleted.Should().NotBeNull();
            result.DataLock.First().JobsToBeDeleted.Should().HaveCount(jobjsToBeDeleted.Count);
            result.DataLock.First().JobsToBeDeleted.First().DcJobId.Should().Be(1);

            result.EarningAudit.Should().NotBeNull();
            result.EarningAudit.Should().BeOfType<List<EarningAuditData>>();
            result.EarningAudit.First().JobsToBeDeleted.Should().NotBeNull();
            result.EarningAudit.First().JobsToBeDeleted.Should().HaveCount(jobjsToBeDeleted.Count);
            result.EarningAudit.First().JobsToBeDeleted.First().DcJobId.Should().Be(1);

            result.RequiredPayments.Should().NotBeNull();
            result.RequiredPayments.Should().BeOfType<List<RequiredPaymentAuditData>>();
            result.RequiredPayments.First().JobsToBeDeleted.Should().NotBeNull();
            result.RequiredPayments.First().JobsToBeDeleted.Should().HaveCount(jobjsToBeDeleted.Count);
            result.RequiredPayments.First().JobsToBeDeleted.First().DcJobId.Should().Be(1);

            _mockLogger.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.AtLeastOnce);
        }

        // Ensures TriggerAuditDataCleanUp returns null and logs when both previous and current periods return no batches.
        [Test]
        public async Task TriggerAuditDataCleanUp_ShouldReturnNullAndLog_WhenNoBatchesAcrossAllPeriods()
        {
            SetDevConfigPeriods();

            _auditDataCleanUpDataserviceMock
                .Setup(a => a.GetSubmissionJobsToBeDeletedBatches(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Enumerable.Empty<SubmissionJobsToBeDeletedBatch>());

            var sut = CreateSut();

            var result = await sut.TriggerAuditDataCleanUp();

            result.Should().BeNull();

            _mockLogger.Verify(
                l => l.Log(
                    It.Is<LogLevel>(lvl => lvl == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((_, __) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((_, __) => true)),
                Times.AtLeastOnce);
        }

        // Validates union of previous + current batches and mapping into all four binding lists.
        [Test]
        public async Task TriggerAuditDataCleanUp_ShouldMergePrevAndCurrentAndMapAllLists_WhenBatchesExist()
        {
            SetDevConfigPeriods();

            var prev = new[] { Batch(1), Batch(2) };
            var cur = new[] { Batch(3) };

            _auditDataCleanUpDataserviceMock
                .Setup(a => a.GetSubmissionJobsToBeDeletedBatches("07", "2324"))
                .ReturnsAsync(prev);
            _auditDataCleanUpDataserviceMock
                .Setup(a => a.GetSubmissionJobsToBeDeletedBatches("08", "2324"))
                .ReturnsAsync(cur);

            var sut = CreateSut();

            var result = await sut.TriggerAuditDataCleanUp();

            result.Should().NotBeNull();
            result!.DataLock.Should().HaveCount(3);
            result.EarningAudit.Should().HaveCount(3);
            result.FundingSource.Should().HaveCount(3);
            result.RequiredPayments.Should().HaveCount(3);

            var ids = result.DataLock.SelectMany(x => x.JobsToBeDeleted.Select(j => j.DcJobId)).ToArray();
            ids.Should().BeEquivalentTo(new[] { 1, 2, 3 });
        }

        // If previous period settings are missing, only the current period is queried and mapped.
        [Test]
        public async Task TriggerAuditDataCleanUp_ShouldQueryOnlyCurrent_WhenPreviousPeriodConfigMissing()
        {
            SetDevConfigPeriods(prevPeriod: "", prevAy: "", curPeriod: "08", curAy: "2324");

            var onlyCurrent = new[] { Batch(10), Batch(20) };

            _auditDataCleanUpDataserviceMock
                .Setup(a => a.GetSubmissionJobsToBeDeletedBatches("08", "2324"))
                .ReturnsAsync(onlyCurrent);

            var sut = CreateSut();

            var result = await sut.TriggerAuditDataCleanUp();

            result.Should().NotBeNull();
            result!.DataLock.Should().HaveCount(2);

            _auditDataCleanUpDataserviceMock.Verify(
                a => a.GetSubmissionJobsToBeDeletedBatches("08", "2324"),
                Times.Once);

            _auditDataCleanUpDataserviceMock.Verify(
                a => a.GetSubmissionJobsToBeDeletedBatches(
                    It.Is<string>(s => string.IsNullOrWhiteSpace(s)),
                    It.Is<string>(s => string.IsNullOrWhiteSpace(s))),
                Times.Never);
        }

        // Confirms Dev path reads period/AY from IConfiguration (not environment variables).
        [Test]
        public async Task TriggerAuditDataCleanUp_ShouldReadPeriodsFromConfiguration_WhenInDevelopment()
        {
            SetDevConfigPeriods(prevPeriod: "05", prevAy: "2223", curPeriod: "06", curAy: "2223");

            _auditDataCleanUpDataserviceMock
                .Setup(a => a.GetSubmissionJobsToBeDeletedBatches("05", "2223"))
                .ReturnsAsync(new[] { Batch(100) });

            _auditDataCleanUpDataserviceMock
                .Setup(a => a.GetSubmissionJobsToBeDeletedBatches("06", "2223"))
                .ReturnsAsync(Enumerable.Empty<SubmissionJobsToBeDeletedBatch>());

            var sut = CreateSut();

            var result = await sut.TriggerAuditDataCleanUp();

            result.Should().NotBeNull();
            result!.DataLock.Should().HaveCount(1);

            _auditDataCleanUpDataserviceMock.Verify(
                a => a.GetSubmissionJobsToBeDeletedBatches("05", "2223"),
                Times.Once);
        }

        // Confirms non-Dev path reads period/AY from environment variables.
        [Test]
        public async Task TriggerAuditDataCleanUp_ShouldReadPeriodsFromEnvironment_WhenNotInDevelopment()
        {
            SetNonDevEnvPeriods(prevPeriod: "01", prevAy: "2425", curPeriod: "02", curAy: "2425");

            _auditDataCleanUpDataserviceMock
                .Setup(a => a.GetSubmissionJobsToBeDeletedBatches("01", "2425"))
                .ReturnsAsync(new[] { Batch(7) });

            _auditDataCleanUpDataserviceMock
                .Setup(a => a.GetSubmissionJobsToBeDeletedBatches("02", "2425"))
                .ReturnsAsync(new[] { Batch(8) });

            var sut = CreateSut();

            var result = await sut.TriggerAuditDataCleanUp();

            result.Should().NotBeNull();
            result!.RequiredPayments.Should().HaveCount(2);

            _auditDataCleanUpDataserviceMock.Verify(
                a => a.GetSubmissionJobsToBeDeletedBatches("01", "2425"),
                Times.Once);
            _auditDataCleanUpDataserviceMock.Verify(
                a => a.GetSubmissionJobsToBeDeletedBatches("02", "2425"),
                Times.Once);
        }

        // Ensures each of the 4 binding lists has an entry per batch returned.
        [Test]
        public async Task TriggerAuditDataCleanUp_ShouldPopulateAllBindingLists_WhenMultipleBatchesReturned()
        {
            SetDevConfigPeriods();

            var batches = new[] { Batch(11), Batch(22), Batch(33), Batch(44) };

            _auditDataCleanUpDataserviceMock
                .Setup(a => a.GetSubmissionJobsToBeDeletedBatches("07", "2324"))
                .ReturnsAsync(batches);
            _auditDataCleanUpDataserviceMock
                .Setup(a => a.GetSubmissionJobsToBeDeletedBatches("08", "2324"))
                .ReturnsAsync(Enumerable.Empty<SubmissionJobsToBeDeletedBatch>());

            var sut = CreateSut();

            var result = await sut.TriggerAuditDataCleanUp();

            result.Should().NotBeNull();
            result!.DataLock.Should().HaveCount(4);
            result.EarningAudit.Should().HaveCount(4);
            result.FundingSource.Should().HaveCount(4);
            result.RequiredPayments.Should().HaveCount(4);
        }
    }
}
