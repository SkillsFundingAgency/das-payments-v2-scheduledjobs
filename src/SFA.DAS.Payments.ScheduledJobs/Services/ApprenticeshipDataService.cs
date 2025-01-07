using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Payments.Application.Infrastructure.Telemetry;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.ScheduledJobs.DataContext;
using SFA.DAS.Payments.ScheduledJobs.Enums;

namespace SFA.DAS.Payments.ScheduledJobs.Services
{
    public class ApprenticeshipDataService : IApprenticeshipDataService
    {
        private const string DasApproved = "DasApproved";
        private const string DasStopped = "DasStopped";
        private const string DasPaused = "DasPaused";
        private const string PaymentsApproved = "PaymentsApproved";
        private const string PaymentsStopped = "PaymentsStopped";
        private const string PaymentsPaused = "PaymentsPaused";
        private const string ApprovalsReferenceDataComparisonEvent = "ApprovalsReferenceDataComparisonEvent";
        private const int DaysToLookBack = 30;

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ITelemetry _telemetry;

        public ApprenticeshipDataService(ITelemetry telemetry, IServiceScopeFactory serviceScopeFactory)
        {
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        public async Task ProcessComparison()
        {
            try
            {
                var pastThirtyDays = DateTime.UtcNow.AddDays(-DaysToLookBack).Date;

                var commitmentsApprovedTask = Task.Run(() => GetCommitmentsApprovedCount(pastThirtyDays));
                var commitmentsStoppedTask = Task.Run(() => GetCommitmentsStoppedCount(pastThirtyDays));
                var commitmentsPausedTask = Task.Run(() => GetCommitmentsPausedCount(pastThirtyDays));
                var paymentsApprovedTask = Task.Run(() => GetPaymentsApprovedCount(pastThirtyDays));
                var paymentsStoppedTask = Task.Run(() => GetPaymentsStoppedCount(pastThirtyDays));
                var paymentsPausedTask = Task.Run(() => GetPaymentsPausedCount(pastThirtyDays));

                await Task.WhenAll(commitmentsApprovedTask, commitmentsStoppedTask, commitmentsPausedTask, paymentsApprovedTask, paymentsStoppedTask, paymentsPausedTask).ConfigureAwait(false);

                TrackTelemetryEvent(commitmentsApprovedTask.Result, commitmentsStoppedTask.Result, commitmentsPausedTask.Result, paymentsApprovedTask.Result, paymentsStoppedTask.Result, paymentsPausedTask.Result);
            }
            catch (Exception ex)
            {
                _telemetry.TrackEvent("Exception", new Dictionary<string, string> { { "ExceptionMessage", ex.Message } }, new Dictionary<string, double>());
                throw;
            }
        }

        private async Task<int> GetCommitmentsApprovedCount(DateTime pastThirtyDays)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CommitmentsDataContext>();
            return await context.Apprenticeship.Include(x => x.Commitment)
                .CountAsync(commitmentsApprenticeship =>
                    commitmentsApprenticeship.Commitment.EmployerAndProviderApprovedOn > pastThirtyDays
                    && (commitmentsApprenticeship.Commitment.Approvals == 3 || commitmentsApprenticeship.Commitment.Approvals == 7));
        }

        private async Task<int> GetCommitmentsStoppedCount(DateTime pastThirtyDays)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CommitmentsDataContext>();
            return await context.Apprenticeship
                .CountAsync(commitmentsApprenticeship =>
                    commitmentsApprenticeship.StopDate > pastThirtyDays
                    && commitmentsApprenticeship.PaymentStatus == PaymentStatus.Withdrawn);
        }

        private async Task<int> GetCommitmentsPausedCount(DateTime pastThirtyDays)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CommitmentsDataContext>();
            return await context.Apprenticeship
                .CountAsync(commitmentsApprenticeship =>
                    commitmentsApprenticeship.IsApproved
                    && commitmentsApprenticeship.PauseDate > pastThirtyDays
                    && commitmentsApprenticeship.PaymentStatus == PaymentStatus.Paused);
        }

        private async Task<int> GetPaymentsApprovedCount(DateTime pastThirtyDays)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PaymentsDataContext>();
            return await context.Apprenticeship
                .CountAsync(paymentsApprenticeship => paymentsApprenticeship.CreationDate > pastThirtyDays);
        }

        private async Task<int> GetPaymentsStoppedCount(DateTime pastThirtyDays)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PaymentsDataContext>();
            return await context.Apprenticeship
                .CountAsync(paymentsApprenticeship =>
                    paymentsApprenticeship.Status == ApprenticeshipStatus.Stopped
                    && paymentsApprenticeship.StopDate > pastThirtyDays);
        }

        private async Task<int> GetPaymentsPausedCount(DateTime pastThirtyDays)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PaymentsDataContext>();
            return await context.Apprenticeship.Include(x => x.ApprenticeshipPauses)
                .CountAsync(paymentsApprenticeship =>
                    paymentsApprenticeship.Status == ApprenticeshipStatus.Paused
                    && paymentsApprenticeship.ApprenticeshipPauses.Any(pause =>
                        pause.PauseDate > pastThirtyDays
                        && pause.ResumeDate == null));
        }

        private void TrackTelemetryEvent(int commitmentsApprovedCount, int commitmentsStoppedCount, int commitmentsPausedCount, int paymentsApprovedCount, int paymentsStoppedCount, int paymentsPausedCount)
        {
            _telemetry.TrackEvent(ApprovalsReferenceDataComparisonEvent, new Dictionary<string, double>
                {
                    { DasApproved, commitmentsApprovedCount },
                    { DasStopped, commitmentsStoppedCount },
                    { DasPaused, commitmentsPausedCount },
                    { PaymentsApproved, paymentsApprovedCount },
                    { PaymentsStopped, paymentsStoppedCount },
                    { PaymentsPaused, paymentsPausedCount },
                });
        }
    }
}
