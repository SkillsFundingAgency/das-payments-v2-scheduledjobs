using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.ScheduledJobs.Models;

namespace SFA.DAS.Payments.ScheduledJobs.DataContext
{
    public class GsoPaymentsDataContext : PaymentsDataContext
    {
        public virtual DbSet<GsoExternalEarningsIdRow> GsoExternalEarningsIds { get; set; }

        public GsoPaymentsDataContext(DbContextOptions<PaymentsDataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<GsoExternalEarningsIdRow>().HasNoKey();
        }
    }
}
