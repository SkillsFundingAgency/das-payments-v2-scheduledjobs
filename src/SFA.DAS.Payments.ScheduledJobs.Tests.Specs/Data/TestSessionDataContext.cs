using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.Application.Data.Configurations;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.ScheduledJobs.Tests.Specs.Data.Configuration;
using DataLockEventModelConfiguration = SFA.DAS.Payments.ScheduledJobs.Tests.Specs.Data.Configuration.DataLockEventModelConfiguration;

namespace SFA.DAS.Payments.ScheduledJobs.Tests.Specs.Data
{
    public class TestSessionDataContext : DbContext
    {
        private readonly string connectionString;

        public virtual DbSet<DataLockEventModel> DataLockEvents { get; set; }
        public virtual DbSet<EarningEventModel> EarningEvents { get; set; }
        public virtual DbSet<FundingSourceEventModel> FundingSourceEvents { get; set; }
        public virtual DbSet<RequiredPaymentEventModel> RequiredPaymentEvents { get; set; }
        public virtual DbSet<JobModel> Jobs { get; set; }

        public TestSessionDataContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString, options => options.CommandTimeout(600));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("Payments2");
            modelBuilder.ApplyConfiguration(new DataLockEventModelConfiguration());
            modelBuilder.ApplyConfiguration(new DataLockEventNonPayablePeriodFailureModelConfiguration());
            modelBuilder.ApplyConfiguration(new DataLockEventNonPayablePeriodModelConfiguration());
            modelBuilder.ApplyConfiguration(new DataLockEventPayablePeriodModelConfiguration());
            modelBuilder.ApplyConfiguration(new DataLockEventPriceEpisodeModelConfiguration());
            modelBuilder.ApplyConfiguration(new EarningEventModelConfiguration());
            modelBuilder.ApplyConfiguration(new EarningEventPeriodModelConfiguration());
            modelBuilder.ApplyConfiguration(new EarningEventPriceEpisodeModelConfiguration());
            modelBuilder.ApplyConfiguration(new FundingSourceEventModelConfiguration());
            modelBuilder.ApplyConfiguration(new RequiredPaymentEventModelConfiguration());
            modelBuilder.ApplyConfiguration(new JobModelConfiguration());
        }

        public void ClearPaymentsData(long ukprn)
        {
            Database.ExecuteSqlRaw(DeleteUkprnData , ukprn);
        }

        public Task<int> ClearPaymentsDataAsync(long ukprn)
        {
            return Database.ExecuteSqlRawAsync(DeleteUkprnData, ukprn);
        }

        private const string DeleteUkprnData = @"
            delete from Payments2.LevyAccount where AccountId in
	            (select AccountId from Payments2.Apprenticeship where Ukprn = {0})

            delete from Payments2.ApprenticeshipPriceEpisode where ApprenticeshipId in 
	            (select Id from Payments2.Apprenticeship where Ukprn = {0})

            delete from Payments2.ApprenticeshipPause where ApprenticeshipId in 
	            (select Id from Payments2.Apprenticeship where Ukprn = {0})

            delete from Payments2.ApprenticeshipDuplicate where ApprenticeshipId in
	            (select Id from Payments2.Apprenticeship where Ukprn = {0} )

            delete from Payments2.DataLockEventNonPayablePeriodFailures where ApprenticeshipId in
	            (select Id from Payments2.Apprenticeship where Ukprn = {0} )

            delete from Payments2.Apprenticeship where Ukprn = {0}

            delete from Payments2.DataLockEventNonPayablePeriod where DataLockEventId in 
	            (select EventId from Payments2.DataLockEvent where Ukprn = {0})

            delete from Payments2.DataLockEventPayablePeriod where DataLockEventId in 
	            (select EventId from Payments2.DataLockEvent where Ukprn = {0})

            delete from Payments2.DataLockEventPriceEpisode where DataLockEventId in 
	            (select EventId from Payments2.DataLockEvent where Ukprn = {0})

            delete from Payments2.DataLockFailure where Ukprn = {0}

            delete from Payments2.DataLockEvent where Ukprn = {0}

            delete from Payments2.EarningEventPeriod where EarningEventId in 
	            (select EventId from Payments2.EarningEvent where Ukprn = {0})

            delete from Payments2.EarningEventPriceEpisode where EarningEventId in 
	            (select EventId from Payments2.EarningEvent where Ukprn = {0})

            delete from Payments2.EarningEvent where Ukprn = {0}

            delete from Payments2.EmployerProviderPriority where Ukprn = {0}

            delete from Payments2.FundingSourceEvent where Ukprn = {0}

            delete from Payments2.RequiredPaymentEvent where Ukprn = {0}

            delete from Payments2.Payment where Ukprn = {0}

            delete from Payments2.SubmittedLearnerAim where Ukprn = {0}

            delete from Payments2.FundingSourceLevyTransaction where Ukprn = {0}
        ";

        public async Task ClearApprenticeshipData(long apprenticeshipId, long uln)
        {
            const string deleteApprenticeshipData = @"
                delete from Payments2.[ApprenticeshipDuplicate] where ApprenticeshipId in (select Id from Payments2.Apprenticeship where Id = {0} or Uln = {1})
                delete from Payments2.[ApprenticeshipPause] where ApprenticeshipId in (select Id from Payments2.Apprenticeship where Id = {0} or Uln = {1})
                delete from Payments2.[ApprenticeshipPriceEpisode] where ApprenticeshipId in (select Id from Payments2.Apprenticeship where Id = {0} or Uln = {1})
                delete from Payments2.[Apprenticeship] where Id = {0} or Uln = {1}
            ";

            await Database.ExecuteSqlRawAsync(deleteApprenticeshipData, apprenticeshipId, uln).ConfigureAwait(false);
        }


    }
}