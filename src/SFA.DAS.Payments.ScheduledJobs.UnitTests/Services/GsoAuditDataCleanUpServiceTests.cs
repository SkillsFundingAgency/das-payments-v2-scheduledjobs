using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.ScheduledJobs.ServiceBus;

namespace SFA.DAS.Payments.ScheduledJobs.UnitTests.Services
{
    [TestFixture]
    public class GsoAuditDataCleanUpServiceTests
    {
        private const byte GsoCourseType = 3; // CourseType.ShortCourse

        private Mock<ILogger<GsoAuditDataCleanUpService>> _mockLogger;
        private Mock<IServiceBusClientHelper> _mockServiceBusClientHelper;
        private Mock<IGsoAuditDataCleanUpDataService> _gsoAuditDataCleanUpDataServiceMock;
        private IPaymentsDataContext _paymentsDataContext;

        private Mock<IConfiguration> _configuration;
        private Mock<IHostEnvironment> _environment;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger<GsoAuditDataCleanUpService>>();
            _mockServiceBusClientHelper = new Mock<IServiceBusClientHelper>();
            _gsoAuditDataCleanUpDataServiceMock = new Mock<IGsoAuditDataCleanUpDataService>();
            _configuration = new Mock<IConfiguration>();
            _environment = new Mock<IHostEnvironment>(MockBehavior.Strict);

            var options = new DbContextOptionsBuilder<PaymentsDataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _paymentsDataContext = new PaymentsDataContext(options);

            _environment.SetupGet(x => x.EnvironmentName).Returns(Environments.Development);
            SetConfigPeriods();
        }

        private GsoAuditDataCleanUpService CreateSut() =>
            new GsoAuditDataCleanUpService(
                _paymentsDataContext,
                _mockLogger.Object,
                _mockServiceBusClientHelper.Object,
                _gsoAuditDataCleanUpDataServiceMock.Object,
                _configuration.Object,
                _environment.Object);

        // Deterministic Guids so assertions can pin exact values without Guid.NewGuid() noise.
        private static Guid ExternalEarningsId(int seed) => new Guid(seed, 0, 0, new byte[8]);

        private static GsoJobsToBeDeletedBatch Batch(params int[] seeds) =>
            new GsoJobsToBeDeletedBatch
            {
                ExternalEarningsIdsToBeDeleted = seeds.Select(ExternalEarningsId).ToArray()
            };

        private void SetupConfigValue(string key, string value)
        {
            var section = new Mock<IConfigurationSection>();
            section.SetupGet(s => s.Value).Returns(value);
            _configuration.Setup(c => c.GetSection(key)).Returns(section.Object);
        }

        private void SetConfigPeriods(
            string previousPeriod = "07", 
            string previousYear = "2526",
            string currentPeriod = "08", 
            string currentYear = "2526")
        {
            _environment.SetupGet(x => x.EnvironmentName).Returns(Environments.Development);
            SetupConfigValue("PreviousAcademicYearCollectionPeriod", previousPeriod);
            SetupConfigValue("PreviousAcademicYear", previousYear);
            SetupConfigValue("CurrentCollectionPeriod", currentPeriod);
            SetupConfigValue("CurrentAcademicYear", currentYear);
            SetupConfigValue("GsoRequiredPaymentAuditDataCleanUpQueue", "sfa-das-payments-scheduledjobs-gso-requiredpayment");
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("PreviousAcademicYearCollectionPeriod", null);
            Environment.SetEnvironmentVariable("PreviousAcademicYear", null);
            Environment.SetEnvironmentVariable("CurrentCollectionPeriod", null);
            Environment.SetEnvironmentVariable("CurrentAcademicYear", null);
            Environment.SetEnvironmentVariable("GsoRequiredPaymentAuditDataCleanUpQueue", null);
        }

        [Test]
        public async Task CleanUpGsoAuditData_ShouldQueryBothPeriods_WithGsoShortCourseType()
        {
            SetConfigPeriods();

            _gsoAuditDataCleanUpDataServiceMock
                .Setup(a => a.GetGsoDuplicateJobsToBeDeletedBatches(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte>()))
                .ReturnsAsync(Enumerable.Empty<GsoJobsToBeDeletedBatch>());

            var sut = CreateSut();

            await sut.CleanUpGsoAuditData();

            _gsoAuditDataCleanUpDataServiceMock.Verify(
                a => a.GetGsoDuplicateJobsToBeDeletedBatches("07", "2526", GsoCourseType), Times.Once);
            _gsoAuditDataCleanUpDataServiceMock.Verify(
                a => a.GetGsoDuplicateJobsToBeDeletedBatches("08", "2526", GsoCourseType), Times.Once);
        }

        [Test]
        public async Task CleanUpGsoAuditData_ShouldOnlyQueryCurrentPeriod_WhenPreviousPeriodConfigMissing()
        {
            SetConfigPeriods(previousPeriod: "", previousYear: "", currentPeriod: "08", currentYear: "2526");

            _gsoAuditDataCleanUpDataServiceMock
                .Setup(a => a.GetGsoDuplicateJobsToBeDeletedBatches("08", "2526", GsoCourseType))
                .ReturnsAsync(Enumerable.Empty<GsoJobsToBeDeletedBatch>());

            var sut = CreateSut();

            await sut.CleanUpGsoAuditData();

            _gsoAuditDataCleanUpDataServiceMock.Verify(
                a => a.GetGsoDuplicateJobsToBeDeletedBatches("08", "2526", GsoCourseType), Times.Once);
            _gsoAuditDataCleanUpDataServiceMock.Verify(
                a => a.GetGsoDuplicateJobsToBeDeletedBatches(
                    It.Is<string>(s => string.IsNullOrWhiteSpace(s)),
                    It.Is<string>(s => string.IsNullOrWhiteSpace(s)),
                    It.IsAny<byte>()),
                Times.Never);
        }


        [Test]
        public async Task CleanUpGsoAuditData_ShouldNotSendAnyMessages_WhenNoBatchesAcrossBothPeriods()
        {
            SetConfigPeriods();

            _gsoAuditDataCleanUpDataServiceMock
                .Setup(a => a.GetGsoDuplicateJobsToBeDeletedBatches(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte>()))
                .ReturnsAsync(Enumerable.Empty<GsoJobsToBeDeletedBatch>());

            var sut = CreateSut();

            await sut.CleanUpGsoAuditData();

            _mockServiceBusClientHelper.Verify(
                s => s.SendMessageToQueueAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task CleanUpGsoAuditData_ShouldSendOneMessagePerExternalEarningsId_WhenBatchesExistAcrossBothPeriods()
        {
            SetConfigPeriods();

            _gsoAuditDataCleanUpDataServiceMock
                .Setup(a => a.GetGsoDuplicateJobsToBeDeletedBatches("07", "2526", GsoCourseType))
                .ReturnsAsync(new[] { Batch(1), Batch(2) });
            _gsoAuditDataCleanUpDataServiceMock
                .Setup(a => a.GetGsoDuplicateJobsToBeDeletedBatches("08", "2526", GsoCourseType))
                .ReturnsAsync(new[] { Batch(3) });

            var sut = CreateSut();

            await sut.CleanUpGsoAuditData();

            _mockServiceBusClientHelper.Verify(
                s => s.SendMessageToQueueAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
        }

        [Test]
        public async Task CleanUpGsoAuditData_ShouldSendASeparateMessagePerId_WhenABatchHasMultipleIds()
        {
            SetConfigPeriods();

            _gsoAuditDataCleanUpDataServiceMock
                .Setup(a => a.GetGsoDuplicateJobsToBeDeletedBatches("07", "2526", GsoCourseType))
                .ReturnsAsync(new[] { Batch(42, 43) });
            _gsoAuditDataCleanUpDataServiceMock
                .Setup(a => a.GetGsoDuplicateJobsToBeDeletedBatches("08", "2526", GsoCourseType))
                .ReturnsAsync(Enumerable.Empty<GsoJobsToBeDeletedBatch>());

            var sut = CreateSut();

            await sut.CleanUpGsoAuditData();

            _mockServiceBusClientHelper.Verify(
                s => s.SendMessageToQueueAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Test]
        public async Task CleanUpGsoAuditData_ShouldSerializeEachMessageAsASingleItemBatch()
        {
            SetConfigPeriods();

            _gsoAuditDataCleanUpDataServiceMock
                .Setup(a => a.GetGsoDuplicateJobsToBeDeletedBatches("07", "2526", GsoCourseType))
                .ReturnsAsync(new[] { Batch(42, 43) });
            _gsoAuditDataCleanUpDataServiceMock
                .Setup(a => a.GetGsoDuplicateJobsToBeDeletedBatches("08", "2526", GsoCourseType))
                .ReturnsAsync(Enumerable.Empty<GsoJobsToBeDeletedBatch>());

            var capturedMessages = new List<string>();
            _mockServiceBusClientHelper
                .Setup(s => s.SendMessageToQueueAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((_, message) => capturedMessages.Add(message))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            await sut.CleanUpGsoAuditData();

            var deserialised = capturedMessages
                .Select(JsonConvert.DeserializeObject<GsoJobsToBeDeletedBatch>)
                .ToList();

            deserialised.Should().HaveCount(2);
            deserialised.Should().OnlyContain(b => b.ExternalEarningsIdsToBeDeleted.Length == 1);
            deserialised.SelectMany(b => b.ExternalEarningsIdsToBeDeleted)
                .Should().BeEquivalentTo(new[] { ExternalEarningsId(42), ExternalEarningsId(43) });
        }

        [Test]
        public async Task CleanUpGsoAuditData_ShouldLogInformation()
        {
            SetConfigPeriods();

            _gsoAuditDataCleanUpDataServiceMock
                .Setup(a => a.GetGsoDuplicateJobsToBeDeletedBatches(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte>()))
                .ReturnsAsync(new[] { Batch(1) });

            var sut = CreateSut();

            await sut.CleanUpGsoAuditData();

            _mockLogger.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.AtLeastOnce);
        }
    }
}
