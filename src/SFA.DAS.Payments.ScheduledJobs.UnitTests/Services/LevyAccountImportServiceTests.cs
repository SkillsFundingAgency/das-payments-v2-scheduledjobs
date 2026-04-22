using SFA.DAS.Payments.FundingSource.Messages.Commands;

namespace SFA.DAS.Payments.ScheduledJobs.UnitTest.Services
{
    [TestFixture]
    public class LevyAccountImportServiceTests
    {
        private Mock<ILogger<LevyAccountImportService>> _mockLogger;
        private Mock<IServiceBusPublisher> _serviceBusPublisher;
        private LevyAccountImportService _levyAccountImportService;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger<LevyAccountImportService>>();
            _serviceBusPublisher = new Mock<IServiceBusPublisher>();
            _levyAccountImportService = new LevyAccountImportService(_serviceBusPublisher.Object, _mockLogger.Object);
        }

        [Test]
        public async Task RunLevyAccountImport_PublishesToServiceBus()
        {
            // Act
            var result = await _levyAccountImportService.RunLevyAccountImport();

            // Assert
            result.Should().NotBeNull();
            _serviceBusPublisher.Verify(x => x.Publish<ImportEmployerAccounts>(
                    It.Is<ImportEmployerAccounts>(x => x.EventId == result.EventId)), Times.Once);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Published ImportEmployerAccounts EventId")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task RunLevyAccountImport_LogsMessagePublishingError()
        {
            // Arrange
            _serviceBusPublisher.Setup(x => x.Publish<ImportEmployerAccounts>(It.IsAny<ImportEmployerAccounts>()))
                .Throws(new InvalidOperationException("test exception"));

            // Act
            Func<Task> act = async () => await _levyAccountImportService.RunLevyAccountImport(); ;

            // Assert

            act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("test exception");
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Unable to publish ImportEmployerAccounts EventId")),
                    It.Is<InvalidOperationException>(ex => ex.Message == "test exception"),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
