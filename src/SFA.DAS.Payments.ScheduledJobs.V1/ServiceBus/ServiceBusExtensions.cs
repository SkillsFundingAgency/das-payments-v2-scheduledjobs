using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.Payments.ScheduledJobs.V1.ServiceBus
{
    public static class ServiceBusExtensions
    {
        public static IServiceCollection AddServiceBusClientHelper(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton(new ServiceBusClient(connectionString));
            services.AddSingleton<IServiceBusClientHelper, ServiceBusClientHelper>();
            return services;
        }
    }
}
