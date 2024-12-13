using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.Application.Infrastructure.Telemetry;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.ScheduledJobs.V1.DataContext;
using SFA.DAS.Payments.ScheduledJobs.V1.Enums;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Services
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

        private readonly ICommitmentsDataContext _commitmentsDataContext;
        private readonly ITelemetry _telemetry;
        private readonly IPaymentsDataContext _paymentDataContext;


        public ApprenticeshipDataService(ITelemetry telemetry
            , IPaymentsDataContext dataContext,
            ICommitmentsDataContext commitmentsDataContext)
        {
            _telemetry = telemetry;
            _paymentDataContext = dataContext;
            _commitmentsDataContext = commitmentsDataContext;
        }

        public async Task ProcessComparison()
        {
            var pastThirtyDays = DateTime.UtcNow.AddDays(-30).Date;

            var commitmentsApprovedTask = _commitmentsDataContext.Apprenticeship.Include(x => x.Commitment)
                .CountAsync(commitmentsApprenticeship =>
                    commitmentsApprenticeship.Commitment.EmployerAndProviderApprovedOn > pastThirtyDays
                    && (commitmentsApprenticeship.Commitment.Approvals == 3 || commitmentsApprenticeship.Commitment.Approvals == 7));

            var commitmentsStoppedTask = _commitmentsDataContext.Apprenticeship
                .CountAsync(commitmentsApprenticeship =>
                    commitmentsApprenticeship.StopDate > pastThirtyDays
                    && commitmentsApprenticeship.PaymentStatus == PaymentStatus.Withdrawn);

            var commitmentsPausedTask = _commitmentsDataContext.Apprenticeship
                .CountAsync(commitmentsApprenticeship =>
                    commitmentsApprenticeship.IsApproved
                    && commitmentsApprenticeship.PauseDate > pastThirtyDays
                    && commitmentsApprenticeship.PaymentStatus == PaymentStatus.Paused);

            var paymentsApprovedTask = _paymentDataContext.Apprenticeship
                .CountAsync(paymentsApprenticeship => paymentsApprenticeship.CreationDate > pastThirtyDays);

            var paymentsStoppedTask = _paymentDataContext.Apprenticeship
                .CountAsync(paymentsApprenticeship =>
                    paymentsApprenticeship.Status == ApprenticeshipStatus.Stopped
                    && paymentsApprenticeship.StopDate > pastThirtyDays);

            var paymentsPausedTask = _paymentDataContext.Apprenticeship.Include(x => x.ApprenticeshipPauses)
                .CountAsync(paymentsApprenticeship =>
                    paymentsApprenticeship.Status == ApprenticeshipStatus.Paused
                    && paymentsApprenticeship.ApprenticeshipPauses.Any(pause =>
                        pause.PauseDate > pastThirtyDays
                        && pause.ResumeDate == null));

            await Task.WhenAll(commitmentsApprovedTask, commitmentsStoppedTask, commitmentsPausedTask, paymentsApprovedTask, paymentsStoppedTask, paymentsPausedTask).ConfigureAwait(false);

            var commitmentsApprovedCount = commitmentsApprovedTask.Result;
            var commitmentsStoppedCount = commitmentsStoppedTask.Result;
            var commitmentsPausedCount = commitmentsPausedTask.Result;

            var paymentsApprovedCount = paymentsApprovedTask.Result;
            var paymentsStoppedCount = paymentsStoppedTask.Result;
            var paymentsPausedCount = paymentsPausedTask.Result;

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
