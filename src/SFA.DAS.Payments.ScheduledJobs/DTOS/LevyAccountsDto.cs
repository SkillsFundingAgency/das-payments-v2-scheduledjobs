using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.ScheduledJobs.Dtos
{
    public class LevyAccountsDto
    {
        public LevyAccountModel DasLevyAccount { get; set; }
        public LevyAccountModel PaymentsLevyAccount { get; set; }
    }
}
