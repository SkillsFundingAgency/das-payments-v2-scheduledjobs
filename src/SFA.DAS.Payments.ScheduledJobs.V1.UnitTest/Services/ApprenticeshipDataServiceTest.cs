using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SFA.DAS.Payments.Application.Infrastructure.Telemetry;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.ScheduledJobs.V1.DataContext;
using SFA.DAS.Payments.ScheduledJobs.V1.Enums;
using SFA.DAS.Payments.ScheduledJobs.V1.Models;
using SFA.DAS.Payments.ScheduledJobs.V1.Services;
using ApprenticeshipModel = SFA.DAS.Payments.Model.Core.Entities.ApprenticeshipModel;

namespace SFA.DAS.Payments.ScheduledJobs.V1.UnitTest.Services
{
    [TestFixture]
    public class ApprenticeshipDataServiceTests
    {
        private Mock<ITelemetry> _mockTelemetry;
        private IServiceScopeFactory _serviceScopeFactory;
        private DbContextOptions<CommitmentsDataContext> _commitmentsOptions;
        private DbContextOptions<PaymentsDataContext> _paymentsOptions;

        [SetUp]
        public void SetUp()
        {
            _mockTelemetry = new Mock<ITelemetry>();

            _commitmentsOptions = new DbContextOptionsBuilder<CommitmentsDataContext>()
                .UseInMemoryDatabase(databaseName: "CommitmentsDatabase")
                .Options;

            _paymentsOptions = new DbContextOptionsBuilder<PaymentsDataContext>()
                .UseInMemoryDatabase(databaseName: "PaymentsDatabase")
                .Options;

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<CommitmentsDataContext>(options => options.UseInMemoryDatabase("CommitmentsDatabase"))
                .AddDbContext<PaymentsDataContext>(options => options.UseInMemoryDatabase("PaymentsDatabase"))
                .BuildServiceProvider();

            //var scope = serviceProvider.CreateScope();
            //_serviceScopeFactory = Mock.Of<IServiceScopeFactory>(x => x.CreateScope() == scope);

            _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        [Test]
        public async Task ProcessComparison_ShouldTrackTelemetryEvent()
        {
            // Arrange
            var service = new ApprenticeshipDataService(_mockTelemetry.Object, _serviceScopeFactory);
            SeedData();

            // Act
            await service.ProcessComparison();

            // Assert
            _mockTelemetry.Verify(t => t.TrackEvent(It.IsAny<string>(), It.IsAny<Dictionary<string, double>>()), Times.Once);
        }

        private void SeedData()
        {
            using (var context = new CommitmentsDataContext(_commitmentsOptions))
            {
                context.Apprenticeship.AddRange(
                    new Models.ApprenticeshipModel { Commitment = new Commitment { EmployerAndProviderApprovedOn = DateTime.UtcNow.AddDays(-10), Approvals = 3 } },
                    new Models.ApprenticeshipModel { StopDate = DateTime.UtcNow.AddDays(-5), PaymentStatus = PaymentStatus.Withdrawn },
                    new Models.ApprenticeshipModel { IsApproved = true, PauseDate = DateTime.UtcNow.AddDays(-3), PaymentStatus = PaymentStatus.Paused }
                );
                context.SaveChanges();
            }

            using (var context = new PaymentsDataContext(_paymentsOptions))
            {
                var apprenticeships = new List<ApprenticeshipModel>
                {
                    new ApprenticeshipModel
                    {
                        Id = 1,
                        AccountId = 12345,
                        AgreementId = "AG123",
                        AgreedOnDate = DateTime.UtcNow.AddDays(-20),
                        Uln = 9876543210,
                        Ukprn = 12345678,
                        EstimatedStartDate = DateTime.UtcNow.AddMonths(-1),
                        EstimatedEndDate = DateTime.UtcNow.AddMonths(11),
                        StandardCode = 123,
                        ProgrammeType = 1,
                        FrameworkCode = 456,
                        PathwayCode = 789,
                        LegalEntityName = "Legal Entity 1",
                        TransferSendingEmployerAccountId = 67890,
                        StopDate = DateTime.UtcNow.AddMonths(-6),
                        Priority = 1,
                        Status = ApprenticeshipStatus.Active,
                        IsLevyPayer = true,
                        ApprenticeshipPriceEpisodes = new List<ApprenticeshipPriceEpisodeModel>
                        {
                            new ApprenticeshipPriceEpisodeModel { /* Initialize properties */ }
                        },
                        ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy,
                        ApprenticeshipPauses = new List<ApprenticeshipPauseModel>
                        {
                            new ApprenticeshipPauseModel { /* Initialize properties */ }
                        },
                        CreationDate = DateTimeOffset.UtcNow.AddDays(-30)
                    },
                    new ApprenticeshipModel
                    {
                        Id = 2,
                        AccountId = 54321,
                        AgreementId = "AG456",
                        AgreedOnDate = DateTime.UtcNow.AddDays(-15),
                        Uln = 1234567890,
                        Ukprn = 87654321,
                        EstimatedStartDate = DateTime.UtcNow.AddMonths(-2),
                        EstimatedEndDate = DateTime.UtcNow.AddMonths(10),
                        StandardCode = 456,
                        ProgrammeType = 2,
                        FrameworkCode = 789,
                        PathwayCode = 123,
                        LegalEntityName = "Legal Entity 2",
                        TransferSendingEmployerAccountId = 98765,
                        StopDate = DateTime.UtcNow.AddMonths(-5),
                        Priority = 2,
                        Status = ApprenticeshipStatus.Paused,
                        IsLevyPayer = false,
                        ApprenticeshipPriceEpisodes = new List<ApprenticeshipPriceEpisodeModel>
                        {
                            new ApprenticeshipPriceEpisodeModel { /* Initialize properties */ }
                        },
                        ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                        ApprenticeshipPauses = new List<ApprenticeshipPauseModel>
                        {
                            new ApprenticeshipPauseModel { /* Initialize properties */ }
                        },
                        CreationDate = DateTimeOffset.UtcNow.AddDays(-25)
                    }
                };

                context.Apprenticeship.AddRange(apprenticeships);
                context.SaveChanges();
            }
        }
    }
}
