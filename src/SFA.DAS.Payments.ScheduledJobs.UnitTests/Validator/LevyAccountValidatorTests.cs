using SFA.DAS.Payments.Application.Infrastructure.Telemetry;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.ScheduledJobs.Validator;

namespace SFA.DAS.Payments.ScheduledJobs.UnitTests.Validator
{
    [TestFixture]
    public class LevyAccountValidatorTests
    {
        private Mock<ITelemetry> _mockTelemetry;
        private LevyAccountValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _mockTelemetry = new Mock<ITelemetry>();
            _validator = new LevyAccountValidator(_mockTelemetry.Object);
        }

        [Test]
        public void Should_Have_Error_When_DasLevyAccountId_Is_Empty()
        {
            // Arrange
            var dto = new LevyAccountsDto
            {
                DasLevyAccount = new LevyAccountModel { AccountId = 0 },
                PaymentsLevyAccount = new LevyAccountModel { AccountId = 1 }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DasLevyAccount.AccountId);
            _mockTelemetry.Verify(x => x.TrackEvent(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), null), Times.Once);
        }

        [Test]
        public void Should_Have_Error_When_PaymentsLevyAccountId_Is_Empty()
        {
            // Arrange
            var dto = new LevyAccountsDto
            {
                DasLevyAccount = new LevyAccountModel { AccountId = 1 },
                PaymentsLevyAccount = new LevyAccountModel { AccountId = 0 }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PaymentsLevyAccount.AccountId);
            _mockTelemetry.Verify(x => x.TrackEvent(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), null), Times.Once);
        }

        [Test]
        public void Should_Have_Error_When_Balances_Do_Not_Match()
        {
            // Arrange
            var dto = new LevyAccountsDto
            {
                DasLevyAccount = new LevyAccountModel { Balance = 100 },
                PaymentsLevyAccount = new LevyAccountModel { Balance = 200 }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DasLevyAccount.Balance);
        }

        [Test]
        public void Should_Have_Error_When_TransferAllowances_Do_Not_Match()
        {
            // Arrange
            var dto = new LevyAccountsDto
            {
                DasLevyAccount = new LevyAccountModel { TransferAllowance = 50 },
                PaymentsLevyAccount = new LevyAccountModel { TransferAllowance = 100 }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DasLevyAccount.TransferAllowance);
        }

        [Test]
        public void Should_Have_Error_When_IsLevyPayer_Do_Not_Match()
        {
            // Arrange
            var dto = new LevyAccountsDto
            {
                DasLevyAccount = new LevyAccountModel { IsLevyPayer = true },
                PaymentsLevyAccount = new LevyAccountModel { IsLevyPayer = false }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DasLevyAccount.IsLevyPayer);
        }

        [Test]
        public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
        {
            // Arrange
            var dto = new LevyAccountsDto
            {
                DasLevyAccount = new LevyAccountModel { AccountId = 1, Balance = 100, TransferAllowance = 50, IsLevyPayer = true },
                PaymentsLevyAccount = new LevyAccountModel { AccountId = 1, Balance = 100, TransferAllowance = 50, IsLevyPayer = true }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
