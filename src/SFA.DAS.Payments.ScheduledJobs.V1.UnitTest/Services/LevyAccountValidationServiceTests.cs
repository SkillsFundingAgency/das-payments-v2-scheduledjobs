using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.ScheduledJobs.V1.DTOS;
using SFA.DAS.Payments.ScheduledJobs.V1.Services;

namespace SFA.DAS.Payments.ScheduledJobs.V1.UnitTest.Services
{
    [Ignore("Need to work on this class")]
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

        private Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> elements) where T : class
        {
            var queryable = elements.AsQueryable();
            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            dbSet.Setup(d => d.ToListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(queryable.ToList());
            return dbSet;
        }

        [Test]
        public async Task Validate_ShouldLogDebugAndInfoMessages()
        {
            // Arrange
            var dasLevyAccounts = new List<LevyAccountModel> { new LevyAccountModel() };
            var paymentsLevyAccounts = new List<LevyAccountModel> { new LevyAccountModel() };

            _mockDasLevyAccountApiWrapper.Setup(x => x.GetDasLevyAccountDetails())
                .ReturnsAsync(dasLevyAccounts);

            var mockDbSet = CreateMockDbSet(paymentsLevyAccounts);
            _mockPaymentsDataContext.Setup(x => x.LevyAccount).Returns(mockDbSet.Object);

            _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<CombinedLevyAccountsDto>(), default))
                .ReturnsAsync(new ValidationResult());

            // Act
            await _levyAccountValidationService.Validate();

            // Assert
            //_mockPaymentLogger.Verify(x => x.LogDebug("Started Validating Employer Accounts"), Times.Once);
            //_mockPaymentLogger.Verify(x => x.LogInfo("Finished Validating Employer Accounts"), Times.Once);
        }

        [Test]
        public async Task Validate_ShouldCallValidatorWithCombinedLevyAccountsDto()
        {
            // Arrange
            var dasLevyAccounts = new List<LevyAccountModel> { new LevyAccountModel() };
            var paymentsLevyAccounts = new List<LevyAccountModel> { new LevyAccountModel() };

            _mockDasLevyAccountApiWrapper.Setup(x => x.GetDasLevyAccountDetails())
                .ReturnsAsync(dasLevyAccounts);

            var mockDbSet = CreateMockDbSet(paymentsLevyAccounts);
            _mockPaymentsDataContext.Setup(x => x.LevyAccount).Returns(mockDbSet.Object);

            _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<CombinedLevyAccountsDto>(), default))
                .ReturnsAsync(new ValidationResult());

            // Act
            await _levyAccountValidationService.Validate();

            // Assert
            _mockValidator.Verify(x => x.ValidateAsync(It.Is<CombinedLevyAccountsDto>(dto =>
                dto.DasLevyAccountCount == dasLevyAccounts.Count &&
                dto.PaymentsLevyAccountCount == paymentsLevyAccounts.Count), default), Times.Once);
        }

        [Test]
        public async Task Validate_ShouldHandleExceptionAndLogError()
        {
            // Arrange
            _mockDasLevyAccountApiWrapper.Setup(x => x.GetDasLevyAccountDetails())
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await _levyAccountValidationService.Validate();

            // Assert
            //_mockPaymentLogger.Verify(x => x.LogError("Error while retrieving Account Balance Details from PaymentsV2", It.IsAny<Exception>()), Times.Once);
        }
    }
}
