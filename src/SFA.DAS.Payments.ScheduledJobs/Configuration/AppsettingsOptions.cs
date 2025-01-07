namespace SFA.DAS.Payments.ScheduledJobs.Configuration
{
    public class AppsettingsOptions : IAppsettingsOptions
    {
        public bool IsEncrypted { get; set; }
        public Values Values { get; set; }
        public Connectionstrings ConnectionStrings { get; set; }
    }

    public class Values
    {
        public string AzureWebJobsStorage { get; set; }
        public string FUNCTIONS_WORKER_RUNTIME { get; set; }
        public string DataLockAuditDataCleanUpQueue { get; set; }
        public string EarningAuditDataCleanUpQueue { get; set; }
        public string FundingSourceAuditDataCleanUpQueue { get; set; }
        public string LevyAccountSchedule { get; set; }
        public string RequiredPaymentAuditDataCleanUpQueue { get; set; }
        public string ApprenticeshipValidationSchedule { get; set; }
        public string AuditDataCleanUpSchedule { get; set; }
        public string LevyAccountValidationSchedule { get; set; }
        public string Version { get; set; }
        public string EnvironmentName { get; set; }
        public string ServiceName { get; set; }
        public string EstimateSubmissionWindowMetricsSchedule { get; set; }
        public string EndpointName { get; set; }
        public string LevyAccountBalanceEndpoint { get; set; }
        public string ApplicationInsightsInstrumentationKey { get; set; }
        public string ServiceBusConnectionString { get; set; }
        public string DasNServiceBusLicenseKey { get; set; }
        public string AccountApiBatchSize { get; set; }
        public string AccountApiBaseUrl { get; set; }
        public string AccountApiClientId { get; set; }
        public string AccountApiClientSecret { get; set; }
        public string AccountApiIdentifierUri { get; set; }
        public string AccountApiTenant { get; set; }
        public string CurrentCollectionPeriod { get; set; }
        public string CurrentAcademicYear { get; set; }
        public string PreviousAcademicYearCollectionPeriod { get; set; }
        public string PerviousAcademicYear { get; set; }
        public string LogLevel { get; set; }
        public string PreviousAcademicYear { get; set; }
    }

    public class Connectionstrings
    {
        public string ServiceBusConnectionString { get; set; }
        public string CommitmentsConnectionString { get; set; }

        public string PaymentsConnectionString { get; set; }
    }

}

