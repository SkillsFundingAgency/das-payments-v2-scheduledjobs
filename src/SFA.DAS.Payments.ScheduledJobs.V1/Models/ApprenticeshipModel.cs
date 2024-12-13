using SFA.DAS.Payments.ScheduledJobs.V1.Enums;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Models
{
    public class ApprenticeshipModel
    {
        public bool IsApproved { get; set; }

        public virtual long Id { get; set; }
        public virtual long CommitmentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Uln { get; set; }
        public ProgrammeType? ProgrammeType { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public decimal? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string NiNumber { get; set; }
        public string EmployerRef { get; set; }
        public string ProviderRef { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? AgreedOn { get; set; }
        public string EpaOrgId { get; set; }
        public long? CloneOf { get; set; }
        public bool IsProviderSearch { get; set; }
        public Guid? ReservationId { get; set; }
        public long? ContinuationOfId { get; set; }
        public DateTime? OriginalStartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? PauseDate { get; set; }

        public virtual Commitment Commitment { get; set; }
    }
}
