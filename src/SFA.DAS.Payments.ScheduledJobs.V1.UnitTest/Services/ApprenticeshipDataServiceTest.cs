using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.ScheduledJobs.V1.DataContext;
using SFA.DAS.Payments.ScheduledJobs.V1.Enums;
using SFA.DAS.Payments.ScheduledJobs.V1.Models;
using SFA.DAS.Payments.ScheduledJobs.V1.Services;
using ITelemetry = SFA.DAS.Payments.Application.Infrastructure.Telemetry.ITelemetry;

namespace SFA.DAS.Payments.ScheduledJobs.V1.UnitTest.Services
{
    [TestFixture]
    public class ApprenticeshipDataServiceTests
    {
        private Mock<ITelemetry> _mockTelemetry;
        private IServiceScopeFactory _serviceScopeFactory;
        private DbContextOptions<CommitmentsDataContext> _commitmentsOptions;
        private DbContextOptions<PaymentsDataContext> _paymentsOptions;
        private const string EventName = "ApprovalsReferenceDataComparisonEvent";

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

            _mockTelemetry.Verify(x => x.TrackEvent(EventName, It.Is<Dictionary<string, double>>(metrics
                => metrics.Any(metric => metric.Key == "DasApproved" && metric.Value == 3))));

            _mockTelemetry.Verify(x => x.TrackEvent(EventName, It.Is<Dictionary<string, double>>(metrics
                => metrics.Any(metric => metric.Key == "DasStopped" && metric.Value == 3))));

            _mockTelemetry.Verify(x => x.TrackEvent(EventName, It.Is<Dictionary<string, double>>(metrics
               => metrics.Any(metric => metric.Key == "DasPaused" && metric.Value == 1))));

            _mockTelemetry.Verify(x => x.TrackEvent(EventName, It.Is<Dictionary<string, double>>(metrics
                => metrics.Any(metric => metric.Key == "PaymentsApproved" && metric.Value == 4))));

            _mockTelemetry.Verify(x => x.TrackEvent(EventName, It.Is<Dictionary<string, double>>(metrics
               => metrics.Any(metric => metric.Key == "PaymentsStopped" && metric.Value == 1))));

            _mockTelemetry.Verify(x => x.TrackEvent(EventName, It.Is<Dictionary<string, double>>(metrics
                => metrics.Any(metric => metric.Key == "PaymentsPaused" && metric.Value == 3))));
        }

        private void SeedData()
        {
            using (var context = new CommitmentsDataContext(_commitmentsOptions))
            {

                var dasApprenticeships = new List<Models.ApprenticeshipModel>
                {
                    new Models.ApprenticeshipModel { IsApproved = true, Commitment = new Commitment { EmployerAndProviderApprovedOn = DateTime.Now, Approvals = 3} },
                    new Models.ApprenticeshipModel { IsApproved = false, Commitment = new Commitment { EmployerAndProviderApprovedOn = DateTime.Now, Approvals = 3} }, //assert we are using the correct new logic based on query in PV2-2215
                    new Models.ApprenticeshipModel { IsApproved = true, Commitment = new Commitment { EmployerAndProviderApprovedOn = DateTime.Now, Approvals = 7} }, //assert we include transfers

                    new Models.ApprenticeshipModel { PaymentStatus = PaymentStatus.Withdrawn, StopDate = DateTime.Now, Commitment = new Commitment { EmployerAndProviderApprovedOn = DateTime.Now } }, //expected in stopped count
                    new Models.ApprenticeshipModel { PaymentStatus = PaymentStatus.Withdrawn, StopDate = DateTime.Now, Commitment = new Commitment { EmployerAndProviderApprovedOn = DateTime.Now } }, //expected in stopped count
                    new Models.ApprenticeshipModel { PaymentStatus = PaymentStatus.Withdrawn, StopDate = DateTime.Now.AddDays(10), Commitment = new Commitment { EmployerAndProviderApprovedOn = DateTime.Now } }, //expected in stopped count
                    new Models.ApprenticeshipModel { PaymentStatus = PaymentStatus.Withdrawn, StopDate = DateTime.Now.AddDays(-31), Commitment = new Commitment { EmployerAndProviderApprovedOn = DateTime.Now } }, //not expected in stopped count (stop date too old)
                    new Models.ApprenticeshipModel { PaymentStatus = PaymentStatus.Withdrawn, Commitment = new Commitment { EmployerAndProviderApprovedOn = DateTime.Now } }, //not expected in stopped count (null stop date)
                    new Models.ApprenticeshipModel { PaymentStatus = PaymentStatus.Completed, StopDate = DateTime.Now, Commitment = new Commitment { EmployerAndProviderApprovedOn = DateTime.Now } }, //not expected in stopped count (wrong status even though stop date is now)

                    new Models.ApprenticeshipModel { PaymentStatus = PaymentStatus.Paused, IsApproved = true, PauseDate = DateTime.Now },
                    new Models.ApprenticeshipModel { PaymentStatus = PaymentStatus.Withdrawn, IsApproved = true, PauseDate = DateTime.Now },
                    new Models.ApprenticeshipModel { PaymentStatus = PaymentStatus.Paused, IsApproved = false, PauseDate = DateTime.Now },
                    new Models.ApprenticeshipModel { PaymentStatus = PaymentStatus.Paused, IsApproved = true, PauseDate = DateTime.Now.AddDays(-31) },
                };

                context.Apprenticeship.AddRange(dasApprenticeships);
                context.SaveChanges();
            }

            using (var context = new PaymentsDataContext(_paymentsOptions))
            {
                var paymentsApprenticeships = new List<Model.Core.Entities.ApprenticeshipModel>
                {
                    new Model.Core.Entities.ApprenticeshipModel { Status = ApprenticeshipStatus.Active, CreationDate = DateTime.Now },
                    new Model.Core.Entities.ApprenticeshipModel { Status = ApprenticeshipStatus.Active, CreationDate = DateTime.Now },
                    new Model.Core.Entities.ApprenticeshipModel { CreationDate = DateTime.Now }, //status doesn't matter anymore, assert query mirrors this
                    new Model.Core.Entities.ApprenticeshipModel { Status = ApprenticeshipStatus.Active, CreationDate = DateTime.Now.AddDays(-31) },
                    new Model.Core.Entities.ApprenticeshipModel { Status = ApprenticeshipStatus.Stopped, StopDate = DateTime.Now },
                    new Model.Core.Entities.ApprenticeshipModel { Status = ApprenticeshipStatus.Stopped, StopDate = DateTime.Now.AddDays(-31) },
                    new Model.Core.Entities.ApprenticeshipModel { StopDate = DateTime.Now.AddDays(-31) },
                    new Model.Core.Entities.ApprenticeshipModel { Status = ApprenticeshipStatus.Paused, ApprenticeshipPauses = new List<ApprenticeshipPauseModel>{ new ApprenticeshipPauseModel{ PauseDate = DateTime.Now, ResumeDate = null } }},
                    new Model.Core.Entities.ApprenticeshipModel { Status = ApprenticeshipStatus.Paused, ApprenticeshipPauses = new List<ApprenticeshipPauseModel>{ new ApprenticeshipPauseModel{ PauseDate = DateTime.Now, ResumeDate = null } }},
                    new Model.Core.Entities.ApprenticeshipModel { Status = ApprenticeshipStatus.Paused, ApprenticeshipPauses = new List<ApprenticeshipPauseModel>{ new ApprenticeshipPauseModel{ PauseDate = DateTime.Now, ResumeDate = null } }},
                    new Model.Core.Entities.ApprenticeshipModel { Status = ApprenticeshipStatus.Paused, CreationDate = DateTime.Now },
                    new Model.Core.Entities.ApprenticeshipModel { Status = ApprenticeshipStatus.Paused, ApprenticeshipPauses = new List<ApprenticeshipPauseModel>{ new ApprenticeshipPauseModel{ PauseDate = DateTime.Now, ResumeDate = DateTime.Now } }},
                    new Model.Core.Entities.ApprenticeshipModel { Status = ApprenticeshipStatus.Paused, ApprenticeshipPauses = new List<ApprenticeshipPauseModel>{ new ApprenticeshipPauseModel{ PauseDate = DateTime.Now.AddDays(-31), ResumeDate = null } }},
                    new Model.Core.Entities.ApprenticeshipModel { Status = ApprenticeshipStatus.Inactive, ApprenticeshipPauses = new List<ApprenticeshipPauseModel>{ new ApprenticeshipPauseModel{ PauseDate = DateTime.Now, ResumeDate = null } }}
                };

                context.Apprenticeship.AddRange(paymentsApprenticeships);
                context.SaveChanges();
            }
        }
    }
}
