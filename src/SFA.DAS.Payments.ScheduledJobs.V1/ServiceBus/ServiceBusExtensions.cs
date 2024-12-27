using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace SFA.DAS.Payments.ScheduledJobs.V1.ServiceBus
{
    public static class ServiceBusExtensions
    {
        public static IServiceCollection AddServiceBusClientHelper(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton(new ServiceBusClient(connectionString));

            // Polly retry policy
            services.AddSingleton<AsyncRetryPolicy>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<ServiceBusClientHelper>>();
                return Policy.Handle<Exception>()
                             .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                                (exception, timeSpan, retryCount, context) =>
                                                {
                                                    logger.LogWarning($"Retry {retryCount} encountered an error: {exception.Message}. Waiting {timeSpan} before next retry.");
                                                });
            });

            services.AddSingleton<IServiceBusClientHelper, ServiceBusClientHelper>();

            return services;
        }
    }
}
