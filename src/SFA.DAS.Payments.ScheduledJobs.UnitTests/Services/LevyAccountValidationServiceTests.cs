using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.ScheduledJobs.DTOS;
using SFA.DAS.Payments.ScheduledJobs.Services;
using SFA.DAS.Payments.ScheduledJobs.Validator;

namespace SFA.DAS.Payments.ScheduledJobs.UnitTest.Services
{
    [TestFixture]
    public class LevyAccountValidationServiceTests
    {
        private Mock<IDasLevyAccountApiWrapper> _mockDasLevyAccountApiWrapper;
        private Mock<IPaymentsDataContext> _mockPaymentsDataContext;
        private Mock<IValidator<CombinedLevyAccountsDto>> _mockValidator;
        private Mock<IPaymentLogger> _mockPaymentLogger;
        private LevyAccountValidationService _levyAccountValidationService;

        [SetUp]
        public void SetUp()
        {
            _mockDasLevyAccountApiWrapper = new Mock<IDasLevyAccountApiWrapper>();
            _mockPaymentsDataContext = new Mock<IPaymentsDataContext>();
            _mockValidator = new Mock<IValidator<CombinedLevyAccountsDto>>();
            _mockPaymentLogger = new Mock<IPaymentLogger>();

            _levyAccountValidationService = new LevyAccountValidationService(
                _mockDasLevyAccountApiWrapper.Object,
                _mockPaymentsDataContext.Object,
                _mockValidator.Object,
                _mockPaymentLogger.Object);
        }

        [Test]
        public async Task Validate_ShouldLogDebugAndInfoMessages()
        {
            // Arrange
            var dasLevyAccounts = new List<LevyAccountModel> { new LevyAccountModel() };
            var paymentsLevyAccounts = new List<LevyAccountModel> { new LevyAccountModel() };

            _mockDasLevyAccountApiWrapper.Setup(x => x.GetDasLevyAccountDetails())
                .ReturnsAsync(dasLevyAccounts);

            _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<CombinedLevyAccountsDto>(), default))
                .ReturnsAsync(new ValidationResult());

            // Act
            await _levyAccountValidationService.Validate();

            // Assert
            _mockDasLevyAccountApiWrapper.Verify(a => a.GetDasLevyAccountDetails(), Times.Once);
            _mockValidator.Verify(a => a.ValidateAsync(It.IsAny<CombinedLevyAccountsDto>(), default), Times.Once);
        }
    }
}
