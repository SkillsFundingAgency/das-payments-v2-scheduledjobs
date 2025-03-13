using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.ScheduledJobs.Common;

namespace SFA.DAS.Payments.ScheduledJobs.Services
{
    public class DasLevyAccountApiWrapper : IDasLevyAccountApiWrapper
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly IAccountApiClient _accountApiClient;
        private readonly IPaymentLogger _logger;

        string apiAccountBatchSize = string.Empty;

        public DasLevyAccountApiWrapper(IAccountApiClient accountApiClient
            , IPaymentLogger logger
            , IConfiguration configuration
            , IHostEnvironment environment)
        {
            _accountApiClient = accountApiClient ?? throw new ArgumentNullException(nameof(accountApiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentException(nameof(configuration));
            _environment = environment ?? throw new ArgumentException(nameof(environment));


            apiAccountBatchSize = _environment.IsDevelopment()
               ? _configuration.GetValue<string>("AccountApiBatchSize")
               : Environment.GetEnvironmentVariable("AccountApiBatchSize");
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
                if (!int.TryParse(apiAccountBatchSize, out int accountApiBatchSize))
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
                if (!int.TryParse(apiAccountBatchSize, out int accountApiBatchSize))
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
