using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.ScheduledJobs.V1.Common;
using SFA.DAS.Payments.ScheduledJobs.V1.DTOS;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Services
{
    public class LevyAccountValidationService : ILevyAccountValidationService
    {
        private readonly IDasLevyAccountApiWrapper _dasLevyAccountApiWrapper;
        private readonly IPaymentsDataContext _paymentsDataContext;
        private readonly IValidator<CombinedLevyAccountsDto> _validator;
        private readonly IPaymentLogger _paymentLogger;

        public LevyAccountValidationService(
            IDasLevyAccountApiWrapper dasLevyAccountApiWrapper,
            IPaymentsDataContext paymentsDataContext,
            IValidator<CombinedLevyAccountsDto> validator,
            IPaymentLogger paymentLogger)
        {
            _dasLevyAccountApiWrapper = dasLevyAccountApiWrapper ?? throw new ArgumentNullException(nameof(dasLevyAccountApiWrapper));
            _paymentsDataContext = paymentsDataContext ?? throw new ArgumentNullException(nameof(paymentsDataContext));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _paymentLogger = paymentLogger ?? throw new ArgumentNullException(nameof(paymentLogger));
        }

        public async Task Validate()
        {
            var combinedLevyAccountBalance = await GetLevyAccountDetails();

            _paymentLogger.LogDebug("Started Validating Employer Accounts");

            await _validator.ValidateAsync(combinedLevyAccountBalance);

            _paymentLogger.LogInfo("Finished Validating Employer Accounts");
        }

        private async Task<CombinedLevyAccountsDto> GetLevyAccountDetails()
        {
            var dasLevyAccountDetails = GetDasLevyAccountDetails();
            var paymentsLevyAccountDetails = GetPaymentsLevyAccountDetails();

            await Task.WhenAll(dasLevyAccountDetails, paymentsLevyAccountDetails).ConfigureAwait(false);

            return new CombinedLevyAccountsDto(dasLevyAccountDetails.Result, paymentsLevyAccountDetails.Result);
        }

        private async Task<List<LevyAccountModel>> GetDasLevyAccountDetails()
        {
            return await _dasLevyAccountApiWrapper.GetDasLevyAccountDetails();
        }

        private async Task<List<LevyAccountModel>> GetPaymentsLevyAccountDetails()
        {
            _paymentLogger.LogDebug("Started Importing Payments Employer Accounts");

            List<LevyAccountModel> levyAccountModels;

            try
            {
                levyAccountModels = await _paymentsDataContext.LevyAccount.ToListAsync();
                if (levyAccountModels.IsNullOrEmpty())
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                _paymentLogger.LogError("Error while retrieving Account Balance Details from PaymentsV2", e);
                return null;
            }

            _paymentLogger.LogInfo("Finished Importing Payments Employer Accounts");

            return levyAccountModels;
        }
    }
}
