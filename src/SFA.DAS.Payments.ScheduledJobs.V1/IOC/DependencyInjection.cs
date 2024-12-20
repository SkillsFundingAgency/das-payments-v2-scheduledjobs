using ESFA.DC.Logging.Config;
using ESFA.DC.Logging.Config.Interfaces;
using ESFA.DC.Logging.Enums;
using ESFA.DC.Logging.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Infrastructure.Telemetry;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Core.Configuration;
using SFA.DAS.Payments.ScheduledJobs.V1.Configuration;
using SFA.DAS.Payments.ScheduledJobs.V1.DataContext;
using SFA.DAS.Payments.ScheduledJobs.V1.DTOS;
using SFA.DAS.Payments.ScheduledJobs.V1.ServiceBus;
using SFA.DAS.Payments.ScheduledJobs.V1.Services;
using SFA.DAS.Payments.ScheduledJobs.V1.Validator;

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

        public static IServiceCollection AddScopedServices(this IServiceCollection services)
        {
            services.AddScoped<IAuditDataCleanUpService, AuditDataCleanUpService>();
            services.AddScoped<IPaymentLogger, PaymentLogger>();
            //services.AddScoped<IEndpointInstanceFactory, EndpointInstanceFactory>();
            services.AddScoped<ILevyAccountImportService, LevyAccountImportService>();
            services.AddScoped<IApprenticeshipDataService, ApprenticeshipDataService>();
            services.AddScoped<ITelemetry, ApplicationInsightsTelemetry>();
            services.AddScoped<IAccountApiClient, AccountApiClient>();
            services.AddScoped<IDasLevyAccountApiWrapper, DasLevyAccountApiWrapper>();
            services.AddScoped<ILevyAccountValidationService, LevyAccountValidationService>();
            services.AddScoped<IExecutionContext, ESFA.DC.Logging.ExecutionContext>();



            // Register FluentValidation validators
            services.AddTransient<IValidator<LevyAccountsDto>, LevyAccountValidator>();
            services.AddTransient<IValidator<CombinedLevyAccountsDto>, CombinedLevyAccountValidator>();



            services.AddScoped<IPaymentsDataContext, PaymentsDataContext>();
            services.AddScoped<ICommitmentsDataContext, CommitmentsDataContext>();

            return services;
        }

        public static IServiceCollection AddSingletonServices(this IServiceCollection services)
        {
            services.AddSingleton<IConfigurationHelper, ConfigurationHelper>();
            services.AddSingleton<IVersionInfo, VersionInfo>();
            return services;
        }

        public static IServiceCollection AddCommitmentsDataContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CommitmentsDataContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("CommitmentsConnectionString"), sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            });
            return services;
        }

        public static IServiceCollection AddAccountApiConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IAccountApiConfiguration>(provider =>
            {
                var configHelper = provider.GetRequiredService<IConfiguration>();

                return new AccountApiConfiguration
                {
                    ApiBaseUrl = configHelper.GetValue<string>("AccountApiBaseUrl"),
                    ClientId = configHelper.GetValue<string>("AccountApiClientId"),
                    ClientSecret = configHelper.GetValue<string>("AccountApiClientSecret"),
                    IdentifierUri = configHelper.GetValue<string>("AccountApiIdentifierUri"),
                    Tenant = configHelper.GetValue<string>("AccountApiTenant")
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

        public static IServiceCollection ConfigureServiceBusConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var ServiceBusConnectionString = configuration.GetConnectionString("ServiceBusConnectionString");
            if (string.IsNullOrEmpty(ServiceBusConnectionString))
            {
                throw new Exception("ServiceBusConnectionString is not set in the configuration");
            }
            services.AddServiceBusClientHelper(ServiceBusConnectionString);
            return services;
        }

    }
}
