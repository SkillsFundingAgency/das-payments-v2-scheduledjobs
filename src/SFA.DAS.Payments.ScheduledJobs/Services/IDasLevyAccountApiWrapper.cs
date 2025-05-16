using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.ScheduledJobs.Services
{
    public interface IDasLevyAccountApiWrapper
    {
        Task<List<LevyAccountModel>> GetDasLevyAccountDetails();
    }
}