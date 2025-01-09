using System.Collections.Generic;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.Application.Infrastructure.Telemetry;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.ScheduledJobs.DTOS;
using SFA.DAS.Payments.ScheduledJobs.Validator;

namespace SFA.DAS.Payments.ScheduledJobs.UnitTests.Validator
{
    [TestFixture]
    public class CombinedLevyAccountValidatorTests
    {
        private Mock<ITelemetry> _mockTelemetry;
        private Mock<IValidator<LevyAccountsDto>> _mockLevyAccountValidator;
        private CombinedLevyAccountValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _mockTelemetry = new Mock<ITelemetry>();
            _mockLevyAccountValidator = new Mock<IValidator<LevyAccountsDto>>();
            _validator = new CombinedLevyAccountValidator(_mockTelemetry.Object, _mockLevyAccountValidator.Object);
        }

        [Test]
        public void Should_Have_Error_When_IsNullOrEmpty_Is_True()
        {
            // Arrange
            var dto = new CombinedLevyAccountsDto(new List<LevyAccountModel>(), new List<LevyAccountModel>());

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.IsNullOrEmpty);
            _mockTelemetry.Verify(x => x.TrackEvent(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), null), Times.Once);

        }

        [Test]
        public void Should_Not_Have_Error_When_IsNullOrEmpty_Is_False()
        {
            // Arrange
            var dto = new CombinedLevyAccountsDto(new List<LevyAccountModel>(), new List<LevyAccountModel>());

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.PaymentsIsLevyPayerCount);
            result.ShouldNotHaveValidationErrorFor(x => x.DasIsLevyPayerCount);
            result.ShouldNotHaveValidationErrorFor(x => x.PaymentsLevyAccountCount);
            result.ShouldNotHaveValidationErrorFor(x => x.DasLevyAccountCount);
            result.ShouldNotHaveValidationErrorFor(x => x.PaymentsTransferAllowanceTotal);
            result.ShouldNotHaveValidationErrorFor(x => x.DasTransferAllowanceTotal);
            result.ShouldNotHaveValidationErrorFor(x => x.PaymentsLevyAccountBalanceTotal);
            result.ShouldNotHaveValidationErrorFor(x => x.DasLevyAccountBalanceTotal);
            
        }

    }
}
