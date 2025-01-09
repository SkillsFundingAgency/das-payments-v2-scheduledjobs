using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.ScheduledJobs.Configuration;
using SFA.DAS.Payments.ScheduledJobs.ServiceBus;

namespace SFA.DAS.Payments.ScheduledJobs.UnitTest.Services
{
    [TestFixture]
    public class AuditDataCleanUpServiceTests
    {
        private Mock<ILogger<AuditDataCleanUpService>> _mockLogger;
        private Mock<IAppSettingsOptions> _mockSettings;
        private Mock<IServiceBusClientHelper> _mockServiceBusClientHelper;
        private IPaymentsDataContext _paymentsDataContext;
        private Mock<IAuditDataCleanUpDataservice> _auditDataCleanUpDataserviceMock;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger<AuditDataCleanUpService>>();
            _mockSettings = new Mock<IAppSettingsOptions>();
            _mockServiceBusClientHelper = new Mock<IServiceBusClientHelper>();
            _auditDataCleanUpDataserviceMock = new Mock<IAuditDataCleanUpDataservice>();

            var options = new DbContextOptionsBuilder<PaymentsDataContext>()
                .UseInMemoryDatabase(databaseName: "PaymentsDatabase")
                .Options;

            _paymentsDataContext = new PaymentsDataContext(options);
        }


        [Test]
        public async Task TriggerAuditDataCleanUp_ShouldLogInformation()
        {
            // Arrange
            _mockSettings.Setup(s => s.Values).Returns(new Values
            {
                CurrentCollectionPeriod = "2021",
                CurrentAcademicYear = "2021/22"
            });

            var jobjsToBeDeleted = new List<SubmissionJobsToBeDeletedBatch>
            {
                new SubmissionJobsToBeDeletedBatch
                {
                    JobsToBeDeleted = new[] { new SubmissionJobsToBeDeletedModel { DcJobId = 1 } }
                }
            };

            _auditDataCleanUpDataserviceMock.Setup(a => a.GetSubmissionJobsToBeDeletedBatches(It.IsAny<string>(), It.IsAny<string>()).Result)
                .Returns(jobjsToBeDeleted);

            var service = new AuditDataCleanUpService(_paymentsDataContext,
                _mockLogger.Object,
                _mockSettings.Object,
                _mockServiceBusClientHelper.Object,
                _auditDataCleanUpDataserviceMock.Object);

            // Act
            var result = await service.TriggerAuditDataCleanUp();

            // Assert
            result.FundingSource.Should().NotBeNull();
            result.FundingSource.Should().BeOfType<FundingSourceAuditData>();
            result.FundingSource.JobsToBeDeleted.Should().NotBeNull();
            result.FundingSource.JobsToBeDeleted.Should().HaveCount(jobjsToBeDeleted.Count);
            result.FundingSource.JobsToBeDeleted.First().DcJobId.Should().Be(1);

            result.DataLock.Should().NotBeNull();
            result.DataLock.Should().BeOfType<DataLockAuditData>();
            result.DataLock.JobsToBeDeleted.Should().NotBeNull();
            result.DataLock.JobsToBeDeleted.Should().HaveCount(jobjsToBeDeleted.Count);
            result.DataLock.JobsToBeDeleted.First().DcJobId.Should().Be(1);

            result.EarningAudit.Should().NotBeNull();
            result.EarningAudit.Should().BeOfType<EarningAuditData>();
            result.EarningAudit.JobsToBeDeleted.Should().NotBeNull();
            result.EarningAudit.JobsToBeDeleted.Should().HaveCount(jobjsToBeDeleted.Count);
            result.EarningAudit.JobsToBeDeleted.First().DcJobId.Should().Be(1);


            result.RequiredPayments.Should().NotBeNull();
            result.RequiredPayments.Should().BeOfType<RequiredPaymentAuditData>();
            result.RequiredPayments.JobsToBeDeleted.Should().NotBeNull();
            result.RequiredPayments.JobsToBeDeleted.Should().HaveCount(jobjsToBeDeleted.Count);
            result.RequiredPayments.JobsToBeDeleted.First().DcJobId.Should().Be(1);

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
