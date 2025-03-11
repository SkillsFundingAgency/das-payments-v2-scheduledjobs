using ESFA.DC.Logging.Config;
using ESFA.DC.Logging.Config.Interfaces;
using ESFA.DC.Logging.Enums;
using ESFA.DC.Logging.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.Payments.ScheduledJobs.Ioc
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAppSettingsConfiguration(this IServiceCollection services, IHostEnvironment env)
        {
            services.AddSingleton<IAppSettingsOptions>(provider =>
            {
                var configHelper = provider.GetRequiredService<IConfiguration>();

                return new AppSettingsOptions
                {
                    IsEncrypted = env.IsDevelopment() ? configHelper.GetValue<string>("IsEncrypted") : Environment.GetEnvironmentVariable("IsEncrypted").ToString(),
                    ConnectionStrings = new ConnectionStrings
                    {
                        ServiceBusConnectionString = env.IsDevelopment() ? configHelper.GetConnectionString("ServiceBusConnectionString") : Environment.GetEnvironmentVariable("ServiceBusConnectionString"),
                        CommitmentsConnectionString = env.IsDevelopment() ? configHelper.GetConnectionString("CommitmentsConnectionString") : Environment.GetEnvironmentVariable("CommitmentsConnectionString"),
                        PaymentsConnectionString = env.IsDevelopment() ? configHelper.GetConnectionString("PaymentsConnectionString") : Environment.GetEnvironmentVariable("PaymentsConnectionString")
                    },
                    Values = new Values
                    {
                        AccountApiBaseUrl = env.IsDevelopment() ? configHelper.GetValue<string>("AccountApiBaseUrl") : Environment.GetEnvironmentVariable("AccountApiBaseUrl"),
                        AccountApiBatchSize = env.IsDevelopment() ? configHelper.GetValue<string>("AccountApiBatchSize") : Environment.GetEnvironmentVariable("AccountApiBatchSize"),
                        AccountApiClientId = env.IsDevelopment() ? configHelper.GetValue<string>("AccountApiClientId") : Environment.GetEnvironmentVariable("AccountApiClientId"),
                        AccountApiClientSecret = env.IsDevelopment() ? configHelper.GetValue<string>("AccountApiClientSecret") : Environment.GetEnvironmentVariable("AccountApiClientSecret"),
                        AccountApiIdentifierUri = env.IsDevelopment() ? configHelper.GetValue<string>("AccountApiIdentifierUri") : Environment.GetEnvironmentVariable("AccountApiIdentifierUri"),
                        AccountApiTenant = env.IsDevelopment() ? configHelper.GetValue<string>("AccountApiTenant") : Environment.GetEnvironmentVariable("AccountApiTenant"),
                        ApplicationInsightsInstrumentationKey = env.IsDevelopment() ? configHelper.GetValue<string>("ApplicationInsightsInstrumentationKey") : Environment.GetEnvironmentVariable("ApplicationInsightsInstrumentationKey"),
                        AuditDataCleanUpSchedule = env.IsDevelopment() ? configHelper.GetValue<string>("AuditDataCleanUpSchedule") : Environment.GetEnvironmentVariable("AuditDataCleanUpSchedule"),
                        CurrentAcademicYear = env.IsDevelopment() ? configHelper.GetValue<string>("CurrentAcademicYear") : Environment.GetEnvironmentVariable("CurrentAcademicYear"),
                        CurrentCollectionPeriod = env.IsDevelopment() ? configHelper.GetValue<string>("CurrentCollectionPeriod") : Environment.GetEnvironmentVariable("CurrentCollectionPeriod"),
                        DataLockAuditDataCleanUpQueue = env.IsDevelopment() ? configHelper.GetValue<string>("DataLockAuditDataCleanUpQueue") : Environment.GetEnvironmentVariable("DataLockAuditDataCleanUpQueue"),
                        DasNServiceBusLicenseKey = env.IsDevelopment() ? configHelper.GetValue<string>("DasNServiceBusLicenseKey") : Environment.GetEnvironmentVariable("DasNServiceBusLicenseKey"),
                        EarningAuditDataCleanUpQueue = env.IsDevelopment() ? configHelper.GetValue<string>("EarningAuditDataCleanUpQueue") : Environment.GetEnvironmentVariable("EarningAuditDataCleanUpQueue"),
                        EndpointName = env.IsDevelopment() ? configHelper.GetValue<string>("EndpointName") : Environment.GetEnvironmentVariable("EndpointName"),
                        EnvironmentName = env.IsDevelopment() ? configHelper.GetValue<string>("EnvironmentName") : Environment.GetEnvironmentVariable("EnvironmentName"),
                        EstimateSubmissionWindowMetricsSchedule = env.IsDevelopment() ? configHelper.GetValue<string>("EstimateSubmissionWindowMetricsSchedule") : Environment.GetEnvironmentVariable("EstimateSubmissionWindowMetricsSchedule"),
                        FundingSourceAuditDataCleanUpQueue = env.IsDevelopment() ? configHelper.GetValue<string>("FundingSourceAuditDataCleanUpQueue") : Environment.GetEnvironmentVariable("FundingSourceAuditDataCleanUpQueue"),
                        FUNCTIONS_WORKER_RUNTIME = env.IsDevelopment() ? configHelper.GetValue<string>("FUNCTIONS_WORKER_RUNTIME") : Environment.GetEnvironmentVariable("FUNCTIONS_WORKER_RUNTIME"),
                        LevyAccountBalanceEndpoint = env.IsDevelopment() ? configHelper.GetValue<string>("LevyAccountBalanceEndpoint") : Environment.GetEnvironmentVariable("LevyAccountBalanceEndpoint"),
                        LevyAccountSchedule = env.IsDevelopment() ? configHelper.GetValue<string>("LevyAccountSchedule") : Environment.GetEnvironmentVariable("LevyAccountSchedule"),
                        LevyAccountValidationSchedule = env.IsDevelopment() ? configHelper.GetValue<string>("LevyAccountValidationSchedule") : Environment.GetEnvironmentVariable("LevyAccountValidationSchedule"),
                        LogLevel = env.IsDevelopment() ? configHelper.GetValue<string>("LogLevel") : Environment.GetEnvironmentVariable("LogLevel"),
                        PreviousAcademicYear = env.IsDevelopment() ? configHelper.GetValue<string>("PreviousAcademicYear") : Environment.GetEnvironmentVariable("PreviousAcademicYear"),
                        PreviousAcademicYearCollectionPeriod = env.IsDevelopment() ? configHelper.GetValue<string>("PreviousAcademicYearCollectionPeriod") : Environment.GetEnvironmentVariable("PreviousAcademicYearCollectionPeriod"),
                        RequiredPaymentAuditDataCleanUpQueue = env.IsDevelopment() ? configHelper.GetValue<string>("RequiredPaymentAuditDataCleanUpQueue") : Environment.GetEnvironmentVariable("RequiredPaymentAuditDataCleanUpQueue"),
                        ServiceName = env.IsDevelopment() ? configHelper.GetValue<string>("ServiceName") : Environment.GetEnvironmentVariable("ServiceName"),
                        Version = env.IsDevelopment() ? configHelper.GetValue<string>("Version") : Environment.GetEnvironmentVariable("Version"),
                        ApprenticeshipValidationSchedule = env.IsDevelopment() ? configHelper.GetValue<string>("ApprenticeshipValidationSchedule") : Environment.GetEnvironmentVariable("ApprenticeshipValidationSchedule"),
                        AzureWebJobsStorage = env.IsDevelopment() ? configHelper.GetValue<string>("AzureWebJobsStorage") : Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                    }
                };
            });

            return services;
        }

        public static IServiceCollection AddPaymentDatabaseContext(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
        {
            string paymentsConnectionString = env.IsDevelopment() ? configuration.GetConnectionString("PaymentsConnectionString") : Environment.GetEnvironmentVariable("PaymentsConnectionString");

            services.AddDbContext<PaymentsDataContext>(options =>
            {
                options.UseSqlServer(paymentsConnectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            }, ServiceLifetime.Transient);
            services.AddScoped<IPaymentsDataContext, PaymentsDataContext>();
            return services;
        }

        public static IServiceCollection AddCommitmentsDataContext(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
        {
            string commitmentsConnectionString = env.IsDevelopment() ? configuration.GetConnectionString("CommitmentsConnectionString") : Environment.GetEnvironmentVariable("CommitmentsConnectionString");
            services.AddDbContext<CommitmentsDataContext>(options =>
            {
                options.UseSqlServer(commitmentsConnectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            }, ServiceLifetime.Transient);
            services.AddScoped<ICommitmentsDataContext, CommitmentsDataContext>();
            return services;
        }

        public static IServiceCollection AddScopedServices(this IServiceCollection services)
        {
            services.AddScoped<IAuditDataCleanUpService, AuditDataCleanUpService>();
            services.AddScoped<IPaymentLogger, PaymentLogger>();
            services.AddScoped<ILevyAccountImportService, LevyAccountImportService>();
            services.AddScoped<IApprenticeshipDataService, ApprenticeshipDataService>();
            services.AddScoped<ITelemetry, ApplicationInsightsTelemetry>();
            services.AddScoped<IAccountApiClient, AccountApiClient>();
            services.AddScoped<IDasLevyAccountApiWrapper, DasLevyAccountApiWrapper>();
            services.AddScoped<ILevyAccountValidationService, LevyAccountValidationService>();
            services.AddScoped<IExecutionContext, ESFA.DC.Logging.ExecutionContext>();
            services.AddScoped<IAuditDataCleanUpDataservice, AuditDataCleanUpDataservice>();

            // Register FluentValidation validators
            services.AddTransient<IValidator<LevyAccountsDto>, LevyAccountValidator>();
            services.AddTransient<IValidator<CombinedLevyAccountsDto>, CombinedLevyAccountValidator>();

            return services;
        }

        public static IServiceCollection AddSingletonServices(this IServiceCollection services)
        {
            services.AddSingleton<IConfigurationHelper, ConfigurationHelper>();
            services.AddSingleton<IVersionInfo, VersionInfo>();
            return services;
        }

        public static IServiceCollection AddAccountApiConfiguration(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
        {
            services.AddSingleton<IAccountApiConfiguration>(provider =>
            {
                var configHelper = provider.GetRequiredService<IConfiguration>();

                return new AccountApiConfiguration
                {
                    ApiBaseUrl = env.IsDevelopment() ? configHelper.GetValue<string>("AccountApiBaseUrl") : Environment.GetEnvironmentVariable("AccountApiBaseUrl"),
                    ClientId = env.IsDevelopment() ? configHelper.GetValue<string>("AccountApiClientId") : Environment.GetEnvironmentVariable("AccountApiClientId"),
                    ClientSecret = env.IsDevelopment() ? configHelper.GetValue<string>("AccountApiClientSecret") : Environment.GetEnvironmentVariable("AccountApiClientSecret"),
                    IdentifierUri = env.IsDevelopment() ? configHelper.GetValue<string>("AccountApiIdentifierUri") : Environment.GetEnvironmentVariable("AccountApiIdentifierUri"),
                    Tenant = env.IsDevelopment() ? configHelper.GetValue<string>("AccountApiTenant") : Environment.GetEnvironmentVariable("AccountApiTenant")
                };
            });

            return services;
        }

        public static IServiceCollection AddApplicationLoggerSettings(this IServiceCollection services)
        {
            services.AddSingleton<IApplicationLoggerSettings>(provider =>
            {
                var versionInfo = provider.GetRequiredService<IVersionInfo>();
                var configHelper = provider.GetRequiredService<IConfigurationHelper>();

                if (!Enum.TryParse(configHelper.GetSettingOrDefault("LogLevel", "Information"), out LogLevel logLevel))
                {
                    logLevel = LogLevel.Information;
                }

                return new ApplicationLoggerSettings
                {
                    ApplicationLoggerOutputSettingsCollection = new List<IApplicationLoggerOutputSettings>
                    {
                        new ConsoleApplicationLoggerOutputSettings
                        {
                            MinimumLogLevel = logLevel
                        },
                    },
                    TaskKey = versionInfo.ServiceReleaseVersion
                };
            });

            return services;
        }

        public static IServiceCollection ConfigureServiceBusConfiguration(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
        {
            var serviceBusConnectionString = env.IsDevelopment() ? configuration.GetConnectionString("ServiceBusConnectionString") : Environment.GetEnvironmentVariable("ServiceBusConnectionString");
            if (string.IsNullOrEmpty(serviceBusConnectionString))
            {
                throw new Exception("ServiceBusConnectionString is not set in the configuration");
            }
            services.AddServiceBusClientHelper(serviceBusConnectionString);
            return services;
        }

    }
}
