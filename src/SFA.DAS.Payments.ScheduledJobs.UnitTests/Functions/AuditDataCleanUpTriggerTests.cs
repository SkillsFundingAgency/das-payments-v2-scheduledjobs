namespace SFA.DAS.Payments.ScheduledJobs.UnitTests.Functions
{
    [TestFixture]
    public class AuditDataCleanUpTriggerTests
    {
        private Mock<IAuditDataCleanUpService> _mockAuditDataCleanUpService;
        private Mock<ILogger<AuditDataCleanUpTrigger>> _mockLogger;
        private AuditDataCleanUpTrigger _function;

        [SetUp]
        public void SetUp()
        {
            _mockAuditDataCleanUpService = new Mock<IAuditDataCleanUpService>();
            _mockLogger = new Mock<ILogger<AuditDataCleanUpTrigger>>();
            _function = new AuditDataCleanUpTrigger(_mockAuditDataCleanUpService.Object, _mockLogger.Object);
        }

        [Test]
        public async Task HttpTriggerAuditDataCleanUp_Should_Return_OK_When_Data_Cleaned_Up()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var request = context.Request;
            var response = context.Response;

            var auditDataCleanUpBinding = new AuditDataCleanUpBinding();
            _mockAuditDataCleanUpService.Setup(x => x.TriggerAuditDataCleanUp())
                .ReturnsAsync(auditDataCleanUpBinding);

            // Act
            var result = await _function.HttpTriggerAuditDataCleanUp(request);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(auditDataCleanUpBinding);
            result.Should().BeOfType<AuditDataCleanUpBinding>();
            result.Should().BeEquivalentTo(auditDataCleanUpBinding);
            response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Test]
        public async Task HttpTriggerAuditDataCleanUp_Should_Return_BadRequest_When_No_Data_Found()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var request = context.Request;
            var response = context.Response;

            _mockAuditDataCleanUpService.Setup(x => x.TriggerAuditDataCleanUp())
                .ReturnsAsync((AuditDataCleanUpBinding)null);

            // Act
            var result = await _function.HttpTriggerAuditDataCleanUp(request);

            // Assert
            response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            result.Should().BeNull();
        }

        [Test]
        public async Task HttpTriggerAuditDataCleanUp_Should_Return_InternalServerError_On_Exception()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var request = context.Request;
            var response = context.Response;

            _mockAuditDataCleanUpService.Setup(x => x.TriggerAuditDataCleanUp())
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _function.HttpTriggerAuditDataCleanUp(request);

            // Assert
            response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            result.Should().BeNull();
        }

        [Test]
        public async Task Run_Should_Call_AuditDataCleanUpTimertrigger()
        {
            // Arrange
            TimerInfo timerInfo = new TimerInfo();
            timerInfo.ScheduleStatus = new ScheduleStatus();

            var auditDataCleanUpBinding = new AuditDataCleanUpBinding();
            _mockAuditDataCleanUpService.Setup(x => x.TriggerAuditDataCleanUp())
                                .ReturnsAsync(new AuditDataCleanUpBinding
                                {
                                    DataLock = new List<DataLockAuditData>(),
                                    EarningAudit = new List<EarningAuditData>(),
                                    FundingSource = new List<FundingSourceAuditData>(),
                                    RequiredPayments = new List<RequiredPaymentAuditData>()
                                });

            // Act
            var result = await _function.AuditDataCleanUp(timerInfo);

            // Assert
            _mockAuditDataCleanUpService.Verify(x => x.TriggerAuditDataCleanUp(), Times.Once);
        }

    }
}
