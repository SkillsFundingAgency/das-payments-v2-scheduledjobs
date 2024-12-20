using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.ScheduledJobs.V1.Common;
using SFA.DAS.Payments.ScheduledJobs.V1.Configuration;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Services
{
    public class DasLevyAccountApiWrapper : IDasLevyAccountApiWrapper
    {
        private readonly IAppsettingsOptions _appSettings;
        private readonly IAccountApiClient _accountApiClient;
        private readonly IPaymentLogger _logger;

        public DasLevyAccountApiWrapper(IAccountApiClient accountApiClient
            , IPaymentLogger logger
            , IAppsettingsOptions settings)
        {
            _accountApiClient = accountApiClient ?? throw new ArgumentNullException(nameof(accountApiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appSettings = settings;
        }

        public async Task<List<LevyAccountModel>> GetDasLevyAccountDetails()
        {
            _logger.LogDebug("Started Importing DAS Employer Accounts");

            var dasLevyAccountDetails = new List<LevyAccountModel>();

            var totalPages = await GetTotalPageSize();

            for (var pageNumber = 1; pageNumber <= totalPages; pageNumber++)
            {
                var levyAccountDetails = await GetPageOfLevyAccounts(pageNumber);

                dasLevyAccountDetails.AddRange(levyAccountDetails);
            }

            _logger.LogInfo("Finished Importing DAS Employer Accounts");

            return dasLevyAccountDetails.IsNullOrEmpty() ? null : dasLevyAccountDetails;
        }

        private async Task<List<LevyAccountModel>> GetPageOfLevyAccounts(int pageNumber, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (!int.TryParse(_appSettings.Values.AccountApiBatchSize, out int accountApiBatchSize))
                {
                    throw new InvalidOperationException("Invalid AccountApiBatchSize value in app settings.");
                }

                var pagedAccountsRecords = await _accountApiClient.GetPageOfAccounts(pageNumber, accountApiBatchSize).ConfigureAwait(false);

                return MapToLevyAccountModel(pagedAccountsRecords);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error while retrieving Account Balance Details for Page {pageNumber} of Levy Accounts", e);
                return new List<LevyAccountModel>();
            }
        }

        private async Task<int> GetTotalPageSize()
        {
            try
            {
                if (!int.TryParse(_appSettings.Values.AccountApiBatchSize, out int accountApiBatchSize))
                {
                    throw new InvalidOperationException("Invalid AccountApiBatchSize value in app settings.");
                }
                var pagedAccountsRecord = await _accountApiClient.GetPageOfAccounts(1, accountApiBatchSize).ConfigureAwait(false);

                return pagedAccountsRecord.TotalPages;
            }
            catch (Exception e)
            {
                _logger.LogError("Error while trying to get Total number of Levy Accounts", e);
                return -1;
            }
        }

        private static List<LevyAccountModel> MapToLevyAccountModel(PagedApiResponseViewModel<AccountWithBalanceViewModel> pagedAccountWithBalanceViewModel)
        {
            var levyAccountModels = pagedAccountWithBalanceViewModel.Data.Select(accountDetail => new LevyAccountModel
            {
                AccountId = accountDetail.AccountId,
                IsLevyPayer = accountDetail.IsLevyPayer,
                AccountName = accountDetail.AccountName,
                Balance = accountDetail.Balance,
                TransferAllowance = accountDetail.RemainingTransferAllowance,
            }).ToList();

            return levyAccountModels;
        }
    }
}
