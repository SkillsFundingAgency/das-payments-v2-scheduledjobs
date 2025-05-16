using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.ScheduledJobs.Models;

namespace SFA.DAS.Payments.ScheduledJobs.DataContext
{
    public interface ICommitmentsDataContext
    {
        DbSet<ApprenticeshipModel> Apprenticeship { get; set; }
    }
}