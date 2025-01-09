using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.ScheduledJobs.DTOS;

namespace SFA.DAS.Payments.ScheduledJobs.UnitTest.Services
{
    [TestFixture]
    public class LevyAccountValidationServiceTests
    {
        private Mock<IDasLevyAccountApiWrapper> _mockDasLevyAccountApiWrapper;
        private IPaymentsDataContext _paymentsDataContext;
        private Mock<IValidator<CombinedLevyAccountsDto>> _mockValidator;
        private Mock<IPaymentLogger> _mockPaymentLogger;
        private LevyAccountValidationService _levyAccountValidationService;

        [SetUp]
        public void SetUp()
        {
            _mockDasLevyAccountApiWrapper = new Mock<IDasLevyAccountApiWrapper>();
            _mockValidator = new Mock<IValidator<CombinedLevyAccountsDto>>();
            _mockPaymentLogger = new Mock<IPaymentLogger>();

            var options = new DbContextOptionsBuilder<PaymentsDataContext>()
                .UseInMemoryDatabase(databaseName: "PaymentsDatabase")
                .Options;

            _paymentsDataContext = new PaymentsDataContext(options);

            _levyAccountValidationService = new LevyAccountValidationService(
                _mockDasLevyAccountApiWrapper.Object,
               _paymentsDataContext,
                _mockValidator.Object,
                _mockPaymentLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            var levyAccount = _paymentsDataContext.LevyAccount.FirstOrDefault(l => l.AccountId == 1);
            if (levyAccount != null)
            {
                _paymentsDataContext.LevyAccount.Remove(levyAccount);
                _paymentsDataContext.SaveChanges();
            }
        }

        private void SeedLevyAccount()
        {
            _paymentsDataContext.LevyAccount.Add(new LevyAccountModel
            {
                AccountId = 1,
                AccountName = "Test",
                Balance = 0,
                IsLevyPayer = true,
                TransferAllowance = 1
            });

            _paymentsDataContext.SaveChanges();
        }

        [Test]
        public async Task Validate_ShouldValidateAsync()
        {
            // Arrange
            var dasLevyAccounts = new List<LevyAccountModel> { new LevyAccountModel() };
            var paymentsLevyAccounts = new List<LevyAccountModel> { new LevyAccountModel() };

            _mockDasLevyAccountApiWrapper.Setup(x => x.GetDasLevyAccountDetails())
                .ReturnsAsync(dasLevyAccounts);

            _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<CombinedLevyAccountsDto>(), default))
                .ReturnsAsync(new ValidationResult());

            SeedLevyAccount();

            // Act
            await _levyAccountValidationService.Validate();

            // Assert
            _mockValidator.Verify(a => a.ValidateAsync(It.IsAny<CombinedLevyAccountsDto>(), default), Times.Once);
        }

        [Test]
        public async Task Validate_ShouldCallGetDasLevyAccountDetails()
        {
            // Arrange
            var dasLevyAccounts = new List<LevyAccountModel> { new LevyAccountModel() };
            var paymentsLevyAccounts = new List<LevyAccountModel> { new LevyAccountModel() };

            _mockDasLevyAccountApiWrapper.Setup(x => x.GetDasLevyAccountDetails())
                .ReturnsAsync(dasLevyAccounts);

            _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<CombinedLevyAccountsDto>(), default))
                .ReturnsAsync(new ValidationResult());

            SeedLevyAccount();

            // Act
            await _levyAccountValidationService.Validate();

            // Assert
            _mockDasLevyAccountApiWrapper.Verify(a => a.GetDasLevyAccountDetails(), Times.Once);
        }


        [Test]
        public async Task Validate_LevyAccountModelsShouldBeNull()
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
        }
    }
}
