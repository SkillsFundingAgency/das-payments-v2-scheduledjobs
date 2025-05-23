﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.ScheduledJobs.Configuration;
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
            _environment = new Mock<IHostEnvironment>();

            var options = new DbContextOptionsBuilder<PaymentsDataContext>()
                .UseInMemoryDatabase(databaseName: "PaymentsDatabase")
                .Options;

            _paymentsDataContext = new PaymentsDataContext(options);
        }


        [Test]
        public async Task TriggerAuditDataCleanUp_ShouldLogInformation()
        {
            // Arrange

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            Environment.SetEnvironmentVariable("CurrentCollectionPeriod", "2021");
            Environment.SetEnvironmentVariable("CurrentAcademicYear", "2021/22");


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
    }
}
