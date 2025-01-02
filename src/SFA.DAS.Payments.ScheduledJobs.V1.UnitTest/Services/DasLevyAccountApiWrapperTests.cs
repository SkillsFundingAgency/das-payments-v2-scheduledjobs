using FluentAssertions;
using Moq;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.ScheduledJobs.V1.Configuration;
using SFA.DAS.Payments.ScheduledJobs.V1.Services;

namespace SFA.DAS.Payments.ScheduledJobs.V1.UnitTest.Services
{
    [TestFixture]
    public class DasLevyAccountApiWrapperTests
    {
        private Mock<IAccountApiClient> _mockAccountApiClient;
        private Mock<IPaymentLogger> _mockLogger;
        private Mock<IAppsettingsOptions> _mockSettings;
        private DasLevyAccountApiWrapper _dasLevyAccountApiWrapper;

        private readonly PagedApiResponseViewModel<AccountWithBalanceViewModel> _apiResponseViewModel = new PagedApiResponseViewModel<AccountWithBalanceViewModel>
        {
            TotalPages = 1,
            Data = new List<AccountWithBalanceViewModel>
            {
                new AccountWithBalanceViewModel
                {
                    AccountId = 1,
                    Balance = 100m,
                    RemainingTransferAllowance = 10m,
                    AccountName = "Test Ltd",
                    IsLevyPayer = true
                }
            }
        };

        [SetUp]
        public void SetUp()
        {
            _mockAccountApiClient = new Mock<IAccountApiClient>();
            _mockLogger = new Mock<IPaymentLogger>();
            _mockSettings = new Mock<IAppsettingsOptions>();

            _mockSettings.Setup(s => s.Values).Returns(new Values
            {
                AccountApiBatchSize = "1000"
            });

            _dasLevyAccountApiWrapper = new DasLevyAccountApiWrapper(_mockAccountApiClient.Object, _mockLogger.Object, _mockSettings.Object);
        }

        [Test]
        public async Task GetDasLevyAccountDetails_Should_CallGetPageOfAccountsToGetLevyAccounts()
        {
            _mockAccountApiClient
                .Setup(x => x.GetPageOfAccounts(1, It.IsAny<int>(), null))
                .ReturnsAsync(_apiResponseViewModel);

            var dasLevyAccountDetails = await _dasLevyAccountApiWrapper.GetDasLevyAccountDetails();

            _mockAccountApiClient
                .Verify(x => x.GetPageOfAccounts(1, It.IsAny<int>(), It.IsAny<DateTime>()), Times.Exactly(0));

            dasLevyAccountDetails.Should().NotBeNullOrEmpty();
            dasLevyAccountDetails.Count.Should().Be(1);
            var levyAccountModel = dasLevyAccountDetails.ElementAt(0);
            levyAccountModel.AccountId.Should().Be(1);
            levyAccountModel.Balance.Should().Be(100m);
            levyAccountModel.TransferAllowance.Should().Be(10m);
            levyAccountModel.IsLevyPayer.Should().Be(true);
        }

        [Test]
        public async Task GetDasLevyAccountDetails_Should_Return_NullWhenGetTotalPageSizeThrowsError()
        {
            _mockAccountApiClient
                .Setup(x => x.GetPageOfAccounts(1, It.IsAny<int>(), It.IsAny<DateTime>()))
                .Throws<Exception>();

            var dasLevyAccountDetails = await _dasLevyAccountApiWrapper.GetDasLevyAccountDetails();

            _mockAccountApiClient
                .Verify(x => x.GetPageOfAccounts(1, It.IsAny<int>(), It.IsAny<DateTime>()), Times.Exactly(0));

            dasLevyAccountDetails.Should().BeNull();
        }

        [Test]
        public async Task GetDasLevyAccountDetails_Should_ReturnNullWhenGetPageOfLevyAccountsThrowsError()
        {
            _mockAccountApiClient
                .SetupSequence(x => x.GetPageOfAccounts(1, It.IsAny<int>(), It.IsAny<DateTime>()))
                .ReturnsAsync(_apiResponseViewModel)
                .Throws<Exception>();

            var dasLevyAccountDetails = await _dasLevyAccountApiWrapper.GetDasLevyAccountDetails();

            _mockAccountApiClient
                .Verify(x => x.GetPageOfAccounts(1, It.IsAny<int>(), It.IsAny<DateTime>()), Times.Exactly(0));

            dasLevyAccountDetails.Should().BeNull();
        }
    }
}
