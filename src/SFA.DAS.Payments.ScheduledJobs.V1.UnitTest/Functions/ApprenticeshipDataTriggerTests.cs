using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Payments.ScheduledJobs.V1.Functions;
using SFA.DAS.Payments.ScheduledJobs.V1.Services;

namespace SFA.DAS.Payments.ScheduledJobs.V1.UnitTest.Functions
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

        //[Test]
        //public async Task TimerTriggerApprenticeshipsReferenceDataComparison_ShouldCallProcessComparison()
        //{
        //    // Arrange
        //    var timerInfo = new TimerInfo(null, new ScheduleStatus(), false);

        //    // Act
        //    await _apprenticeshipDataTrigger.TimerTriggerApprenticeshipsReferenceDataComparison(timerInfo);

        //    // Assert
        //    _mockApprenticeshipDataService.Verify(service => service.ProcessComparison(), Times.Once);
        //}

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
