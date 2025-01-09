namespace SFA.DAS.Payments.ScheduledJobs.UnitTest.Functions
{
    [TestFixture]
    public class ApprenticeshipDataTriggerTests
    {
        private Mock<ILogger<ApprenticeshipDataTrigger>> _mockLogger;
        private Mock<IApprenticeshipDataService> _mockApprenticeshipDataService;
        private ApprenticeshipDataTrigger _apprenticeshipDataTrigger;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger<ApprenticeshipDataTrigger>>();
            _mockApprenticeshipDataService = new Mock<IApprenticeshipDataService>();
            _apprenticeshipDataTrigger = new ApprenticeshipDataTrigger(_mockLogger.Object, _mockApprenticeshipDataService.Object);
        }

        [Test]
        public async Task HttpTriggerApprenticeshipsReferenceDataComparison_ShouldCallProcessComparison()
        {
            // Arrange
            var httpRequest = new Mock<HttpRequest>();

            // Act
            await _apprenticeshipDataTrigger.HttpTriggerApprenticeshipsReferenceDataComparison(httpRequest.Object);

            // Assert
            _mockApprenticeshipDataService.Verify(service => service.ProcessComparison(), Times.Once);
        }
    }
}
