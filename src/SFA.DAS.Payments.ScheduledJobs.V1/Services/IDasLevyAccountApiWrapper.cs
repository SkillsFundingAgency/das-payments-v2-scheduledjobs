using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Services
{
    public interface IDasLevyAccountApiWrapper
    {
        Task<List<LevyAccountModel>> GetDasLevyAccountDetails();
    }
}