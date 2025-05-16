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
        public void Run_Should_Call_LevyAccountImportService()
        {
            // Arrange
            TimerInfo timerInfo = new TimerInfo();

            var levyAccountImportBinding = new LevyAccountImportBinding();
            _mockLevyAccountImportService.Setup(x => x.RunLevyAccountImport())
                .Returns(levyAccountImportBinding);

            // Act
            var result = _function.Run(timerInfo);

            // Assert
            result.Should().BeEquivalentTo(levyAccountImportBinding);
            _mockLevyAccountImportService.Verify(x => x.RunLevyAccountImport(), Times.Once);
        }

        [Test]
        public void HttpLevyAccountImport_Should_Return_Result_When_Successful()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var request = context.Request;
            var response = context.Response;

            var levyAccountImportBinding = new LevyAccountImportBinding();
            _mockLevyAccountImportService.Setup(x => x.RunLevyAccountImport())
                .Returns(levyAccountImportBinding);

            // Act
            var result = _function.HttpLevyAccountImport(request);

            // Assert
            result.Should().NotBeNull();
            response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Test]
        public void HttpLevyAccountImport_Should_Return_InternalServerError_On_Exception()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var request = context.Request;
            var response = context.Response;

            _mockLevyAccountImportService.Setup(x => x.RunLevyAccountImport())
                .Throws(new Exception("Test exception"));

            // Act
            var result = _function.HttpLevyAccountImport(request);

            // Assert
            response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            result.Result.Should().BeNull();
        }
    }
}
