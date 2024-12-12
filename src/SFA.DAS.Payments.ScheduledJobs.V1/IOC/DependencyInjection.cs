using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.ScheduledJobs.V1.Configuration;
using SFA.DAS.Payments.ScheduledJobs.V1.Services;

namespace SFA.DAS.Payments.ScheduledJobs.V1.IOC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAppsettingsConfiguration(this IServiceCollection services)
        {
            services.AddSingleton<IAppsettingsOptions>(provider =>
            {
                var configHelper = provider.GetRequiredService<IConfiguration>();

                return new AppsettingsOptions
                {
                    IsEncrypted = configHelper.GetValue<bool>("IsEncrypted"),
                    ConnectionStrings = new Connectionstrings
                    {
                        ServiceBusConnectionString = configHelper.GetConnectionString("ServiceBusConnectionString"),
                        CommitmentsConnectionString = configHelper.GetConnectionString("CommitmentsConnectionString")
                    },
                    Values = new Values
                    {
                        AccountApiBaseUrl = configHelper.GetValue<string>("AccountApiBaseUrl"),
                        AccountApiBatchSize = configHelper.GetValue<string>("AccountApiBatchSize"),
                        AccountApiClientId = configHelper.GetValue<string>("AccountApiClientId"),
                        AccountApiClientSecret = configHelper.GetValue<string>("AccountApiClientSecret"),
                        AccountApiIdentifierUri = configHelper.GetValue<string>("AccountApiIdentifierUri"),
                        AccountApiTenant = configHelper.GetValue<string>("AccountApiTenant"),
                        ApplicationInsightsInstrumentationKey = configHelper.GetValue<string>("ApplicationInsightsInstrumentationKey"),
                        AuditDataCleanUpSchedule = configHelper.GetValue<string>("AuditDataCleanUpSchedule"),
                        CurrentAcademicYear = configHelper.GetValue<string>("CurrentAcademicYear"),
                        CurrentCollectionPeriod = configHelper.GetValue<string>("CurrentCollectionPeriod"),
                        DataLockAuditDataCleanUpQueue = configHelper.GetValue<string>("DataLockAuditDataCleanUpQueue"),
                        DasNServiceBusLicenseKey = configHelper.GetValue<string>("DasNServiceBusLicenseKey"),
                        EarningAuditDataCleanUpQueue = configHelper.GetValue<string>("EarningAuditDataCleanUpQueue"),
                        EndpointName = configHelper.GetValue<string>("EndpointName"),
                        EnvironmentName = configHelper.GetValue<string>("EnvironmentName"),
                        EstimateSubmissionWindowMetricsSchedule = configHelper.GetValue<string>("EstimateSubmissionWindowMetricsSchedule"),
                        FundingSourceAuditDataCleanUpQueue = configHelper.GetValue<string>("FundingSourceAuditDataCleanUpQueue"),
                        FUNCTIONS_WORKER_RUNTIME = configHelper.GetValue<string>("FUNCTIONS_WORKER_RUNTIME"),
                        LevyAccountBalanceEndpoint = configHelper.GetValue<string>("LevyAccountBalanceEndpoint"),
                        LevyAccountSchedule = configHelper.GetValue<string>("LevyAccountSchedule"),
                        LevyAccountValidationSchedule = configHelper.GetValue<string>("LevyAccountValidationSchedule"),
                        LogLevel = configHelper.GetValue<string>("LogLevel"),
                        PaymentsConnectionString = configHelper.GetValue<string>("PaymentsConnectionString"),
                        PerviousAcademicYear = configHelper.GetValue<string>("PerviousAcademicYear"),
                        PreviousAcademicYearCollectionPeriod = configHelper.GetValue<string>("PreviousAcademicYearCollectionPeriod"),
                        RequiredPaymentAuditDataCleanUpQueue = configHelper.GetValue<string>("RequiredPaymentAuditDataCleanUpQueue"),
                        ServiceBusConnectionString = configHelper.GetValue<string>("ServiceBusConnectionString"),
                        ServiceName = configHelper.GetValue<string>("ServiceName"),
                        Version = configHelper.GetValue<string>("Version"),
                        ApprenticeshipValidationSchedule = configHelper.GetValue<string>("ApprenticeshipValidationSchedule"),
                        AzureWebJobsStorage = configHelper.GetValue<string>("AzureWebJobsStorage")


                    }
                };
            });

            return services;
        }

        public static IServiceCollection AddPaymentDatabaseContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PaymentsDataContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("PaymentsConnectionString"), sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            });
            return services;
        }

        public static IServiceCollection AddScoppedServices(this IServiceCollection services)
        {
            services.AddScoped<IAuditDataCleanUpService, AuditDataCleanUpService>();
            services.AddScoped<IPaymentLogger, PaymentLogger>();
            services.AddScoped<IEndpointInstanceFactory, EndpointInstanceFactory>();
            services.AddScoped<IPaymentsDataContext, PaymentsDataContext>();
            services.AddScoped<ILevyAccountImportService, LevyAccountImportService>();

            return services;
        }
    }
}
