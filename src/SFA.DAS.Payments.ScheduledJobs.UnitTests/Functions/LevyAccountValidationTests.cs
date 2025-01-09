using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.ScheduledJobs.Functions;
using SFA.DAS.Payments.ScheduledJobs.Services;

namespace SFA.DAS.Payments.ScheduledJobs.UnitTests.Functions
{
    [TestFixture]
    public class LevyAccountValidationTests
    {
        private Mock<ILevyAccountValidationService> _mockLevyAccountValidationService;
        private Mock<ILogger<LevyAccountValidation>> _mockLogger;
        private LevyAccountValidation _levyAccountValidation;

        [SetUp]
        public void SetUp()
        {
            _mockLevyAccountValidationService = new Mock<ILevyAccountValidationService>();
            _mockLogger = new Mock<ILogger<LevyAccountValidation>>();
            _levyAccountValidation = new LevyAccountValidation(_mockLevyAccountValidationService.Object, _mockLogger.Object);
        }

        [Test]
        public async Task TimerTriggerLevyAccountValidation_Should_Call_Validate()
        {
            // Arrange
            var timerInfo = new TimerInfo();

            // Act
            await _levyAccountValidation.TimerTriggerLevyAccountValidation(timerInfo);

            // Assert
            _mockLevyAccountValidationService.Verify(x => x.Validate(), Times.Once);
        }

        [Test]
        public async Task HttpTriggerLevyAccountValidation_Should_Call_Validate()
        {
            // Arrange
            var httpRequest = new Mock<HttpRequest>();

            // Act
            await _levyAccountValidation.HttpTriggerLevyAccountValidation(httpRequest.Object);

            // Assert
            _mockLevyAccountValidationService.Verify(x => x.Validate(), Times.Once);
        }
    }
}
