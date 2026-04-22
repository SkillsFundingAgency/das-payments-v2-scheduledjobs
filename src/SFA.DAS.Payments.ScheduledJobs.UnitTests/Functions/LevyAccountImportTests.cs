namespace SFA.DAS.Payments.ScheduledJobs.UnitTests.Functions
{
    [TestFixture]
    public class LevyAccountImportTests
    {
        private Mock<ILevyAccountImportService> _mockLevyAccountImportService;
        private Mock<ILogger<LevyAccountImport>> _mockLogger;
        private LevyAccountImport _function;

        [SetUp]
        public void SetUp()
        {
            _mockLevyAccountImportService = new Mock<ILevyAccountImportService>();
            _mockLogger = new Mock<ILogger<LevyAccountImport>>();
            _function = new LevyAccountImport(_mockLogger.Object, _mockLevyAccountImportService.Object);
        }

        [Test]
        public async Task Run_Should_Call_LevyAccountImportService()
        {
            // Arrange
            var timerInfo = new TimerInfo();

            // Act
            await _function.Run(timerInfo);

            // Assert
            _mockLevyAccountImportService.Verify(x => x.RunLevyAccountImport(), Times.Once);
        }

        [Test]
        public async Task Run_Should_Log_WhenExceptionThrown()
        {
            // Arrange
            var timerInfo = new TimerInfo();
            _mockLevyAccountImportService.Setup(x => x.RunLevyAccountImport())
                .Throws(new Exception("Test exception"));

            // Act
            await _function.Run(timerInfo);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("An error occurred while processing the scheduled levy account import")),
                    It.Is<Exception>(ex => ex.Message == "Test exception"),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task HttpLevyAccountImport_Should_Return_Result_When_Successful()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var request = context.Request;
            var response = context.Response;

            // Act
            await _function.HttpLevyAccountImport(request);

            // Assert
            response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Test]
        public async Task HttpLevyAccountImport_Should_Return_InternalServerError_On_Exception()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var request = context.Request;
            var response = context.Response;

            _mockLevyAccountImportService.Setup(x => x.RunLevyAccountImport())
                .Throws(new Exception("Test exception"));

            // Act
            await _function.HttpLevyAccountImport(request);

            // Assert
            response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("An error occurred while processing the request")), 
                    It.Is<Exception>(ex => ex.Message == "Test exception"),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
