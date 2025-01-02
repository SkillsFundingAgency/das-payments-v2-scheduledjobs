using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Moq;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.ScheduledJobs.V1.Configuration;
using SFA.DAS.Payments.ScheduledJobs.V1.ServiceBus;
using SFA.DAS.Payments.ScheduledJobs.V1.Services;

namespace SFA.DAS.Payments.ScheduledJobs.V1.UnitTest.Services
{
    [TestFixture]
    public class AuditDataCleanUpServiceTests
    {
        private Mock<ILogger<AuditDataCleanUpService>> _mockLogger;
        private Mock<IAppsettingsOptions> _mockSettings;
        private Mock<IServiceBusClientHelper> _mockServiceBusClientHelper;
        private IPaymentsDataContext _paymentsDataContext;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger<AuditDataCleanUpService>>();
            _mockSettings = new Mock<IAppsettingsOptions>();
            _mockServiceBusClientHelper = new Mock<IServiceBusClientHelper>();

            var options = new DbContextOptionsBuilder<PaymentsDataContext>()
                .UseInMemoryDatabase(databaseName: "PaymentsDatabase")
                .Options;

            _paymentsDataContext = new PaymentsDataContext(options);

            SeedData();
        }

        private void SeedData()
        {
            // Add test data to the in-memory database
            _paymentsDataContext.EarningEvent.AddRange(
                new EarningEventModel { Ukprn = 22, StartDate = DateAndTime.Now }
            );
            _paymentsDataContext.RequiredPaymentEvent.AddRange(
                new RequiredPaymentEventModel
                {
                    Ukprn = 22,
                    StartDate = DateAndTime.Now,
                    LearnerReferenceNumber = "12345",
                    LearningAimFundingLineType = "TypeA",
                    LearningAimReference = "Ref123",
                    PriceEpisodeIdentifier = "Episode1"
                }
            );
            _paymentsDataContext.DataLockgEvent.AddRange(
                new DataLockEventModel
                {
                    Ukprn = 22,
                    StartDate = DateAndTime.Now,
                    LearnerReferenceNumber = "12345",
                    LearningAimFundingLineType = "TypeA",
                    LearningAimReference = "Ref123"
                }
            );

            _paymentsDataContext.SaveChanges();
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

            var service = new AuditDataCleanUpService(_paymentsDataContext, _mockLogger.Object, _mockSettings.Object, _mockServiceBusClientHelper.Object);

            // Act
            var result = await service.TriggerAuditDataCleanUp();

            // Assert
            _mockLogger.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true), // It.IsAnyType is used to match any state object
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), // It.IsAnyType is used to match any formatter
                Times.AtLeastOnce);
        }
    }
}
