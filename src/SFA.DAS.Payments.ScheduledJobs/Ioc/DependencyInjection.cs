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
            services.AddSingleton<IAccountApiConfiguration, AccountApiConfiguration>(provider =>
            {
                return new AccountApiConfiguration
                {
                    ApiBaseUrl = env.IsDevelopment() ? configuration.GetValue<string>("AccountApiBaseUrl") : Environment.GetEnvironmentVariable("AccountApiBaseUrl"),
                    ClientId = env.IsDevelopment() ? configuration.GetValue<string>("AccountApiClientId") : Environment.GetEnvironmentVariable("AccountApiClientId"),
                    ClientSecret = env.IsDevelopment() ? configuration.GetValue<string>("AccountApiClientSecret") : Environment.GetEnvironmentVariable("AccountApiClientSecret"),
                    IdentifierUri = env.IsDevelopment() ? configuration.GetValue<string>("AccountApiIdentifierUri") : Environment.GetEnvironmentVariable("AccountApiIdentifierUri"),
                    Tenant = env.IsDevelopment() ? configuration.GetValue<string>("AccountApiTenant") : Environment.GetEnvironmentVariable("AccountApiTenant")
                };
            });

            return services;
        }

        public static IServiceCollection AddApplicationLoggerSettings(this IServiceCollection services)
        {
            services.AddSingleton<IApplicationLoggerSettings, ApplicationLoggerSettings>(provider =>
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
