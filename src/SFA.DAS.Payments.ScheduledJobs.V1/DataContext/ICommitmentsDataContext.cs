using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.ScheduledJobs.V1.Models;

namespace SFA.DAS.Payments.ScheduledJobs.V1.DataContext
{
    public interface ICommitmentsDataContext
    {
        DbSet<ApprenticeshipModel> Apprenticeship { get; set; }
    }
}