using System.Text;
using SFA.DAS.Payments.ScheduledJobs.Services;

namespace SFA.DAS.Payments.ScheduledJobs.UnitTests.Functions
{
    [TestFixture]
    public class GsoAuditDataCleanUpTriggerTests
    {
        private Mock<IGsoAuditDataCleanUpService> _mockGsoAuditDataCleanUpService;
        private Mock<ILogger> _mockLogger;
        private Mock<ILoggerFactory> _mockLoggerFactory;
        private GsoAuditDataCleanUpTrigger _function;

        [SetUp]
        public void SetUp()
        {
            _mockGsoAuditDataCleanUpService = new Mock<IGsoAuditDataCleanUpService>();
            _mockLogger = new Mock<ILogger>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLoggerFactory
                .Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(_mockLogger.Object);

            _function = new GsoAuditDataCleanUpTrigger(_mockLoggerFactory.Object, _mockGsoAuditDataCleanUpService.Object);
        }

        private static string ReadBody(HttpResponse response)
        {
            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body, Encoding.UTF8, leaveOpen: true);
            return reader.ReadToEnd();
        }

        [Test]
        public async Task GsoAuditDataCleanUpTimerTrigger_ShouldCallCleanUpGsoAuditData_WhenScheduleStatusIsNotNull()
        {
            var timerInfo = new TimerInfo { ScheduleStatus = new ScheduleStatus() };

            await _function.GsoAuditDataCleanUpTimerTrigger(timerInfo);

            _mockGsoAuditDataCleanUpService.Verify(x => x.CleanUpGsoAuditData(), Times.Once);
        }

        [Test]
        public async Task GsoAuditDataCleanUpTimerTrigger_ShouldNotCallCleanUpGsoAuditData_WhenScheduleStatusIsNull()
        {
            var timerInfo = new TimerInfo { ScheduleStatus = null };

            await _function.GsoAuditDataCleanUpTimerTrigger(timerInfo);

            _mockGsoAuditDataCleanUpService.Verify(x => x.CleanUpGsoAuditData(), Times.Never);
        }

        [Test]
        public async Task GsoAuditDataCleanUpHttpTrigger_ShouldReturnOk_WhenCleanUpSucceeds()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            var request = context.Request;
            var response = context.Response;

            _mockGsoAuditDataCleanUpService.Setup(x => x.CleanUpGsoAuditData()).Returns(Task.CompletedTask);

            await _function.GsoAuditDataCleanUpHttpTrigger(request);

            response.StatusCode.Should().Be((int)HttpStatusCode.OK);
            ReadBody(response).Should().Be("Request processed successfully");
        }

        [Test]
        public async Task GsoAuditDataCleanUpHttpTrigger_ShouldReturnInternalServerError_WhenCleanUpThrows()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            var request = context.Request;
            var response = context.Response;

            _mockGsoAuditDataCleanUpService
                .Setup(x => x.CleanUpGsoAuditData())
                .ThrowsAsync(new Exception("Test exception"));

            await _function.GsoAuditDataCleanUpHttpTrigger(request);

            response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            ReadBody(response).Should().Contain("Test exception");
        }

        [Test]
        public async Task GsoAuditDataCleanUpHttpTrigger_ShouldLogError_WhenCleanUpThrows()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            var request = context.Request;

            _mockGsoAuditDataCleanUpService
                .Setup(x => x.CleanUpGsoAuditData())
                .ThrowsAsync(new Exception("Test exception"));

            await _function.GsoAuditDataCleanUpHttpTrigger(request);

            _mockLogger.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
