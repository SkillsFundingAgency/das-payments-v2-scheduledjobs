﻿namespace SFA.DAS.Payments.ScheduledJobs.UnitTest.Services
{
    [TestFixture]
    public class LevyAccountImportServiceTests
    {
        private Mock<ILogger<LevyAccountImportService>> _mockLogger;
        private LevyAccountImportService _levyAccountImportService;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger<LevyAccountImportService>>();
            _levyAccountImportService = new LevyAccountImportService(_mockLogger.Object);
        }

        [Test]
        public void RunLevyAccountImport_ShouldReturnLevyAccountImportBinding()
        {
            // Act
            var result = _levyAccountImportService.RunLevyAccountImport();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<LevyAccountImportBinding>();
            result.EventId.Should().NotBeEmpty();
        }
    }
}
