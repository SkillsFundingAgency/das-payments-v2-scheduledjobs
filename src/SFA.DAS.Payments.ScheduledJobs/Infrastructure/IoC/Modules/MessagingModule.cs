using System.Net;
using System.Threading.Tasks;
using Autofac;
using NServiceBus;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.ScheduledJobs.Infrastructure.Configuration;

namespace SFA.DAS.Payments.ScheduledJobs.Infrastructure.IoC.Modules
{
    public class MessagingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MessagingLoggerFactory>();
            builder.RegisterType<MessagingLogger>();

            builder.Register((c, p) =>
                             {
                                 var config = c.Resolve<IScheduledJobsConfiguration>();
                                 var endpointConfiguration = new EndpointConfiguration(config.EndpointName);

                                 var logger = c.Resolve<MessagingLogger>();

                                 endpointConfiguration.CustomDiagnosticsWriter(
                                     (diagnostics, ct) =>
                                     {
                                         logger.Info(diagnostics);
                                         return Task.CompletedTask;
                                     });

                                 var conventions = endpointConfiguration.Conventions();
                                 conventions.DefiningCommandsAs(type => (type.Namespace?.StartsWith("SFA.DAS.Payments") ?? false) && (bool)type.Namespace?.Contains(".Messages.Commands"));

                                 if (!string.IsNullOrEmpty(config.DasNServiceBusLicenseKey))
                                 {
                                     var license = WebUtility.HtmlDecode(config.DasNServiceBusLicenseKey);
                                     endpointConfiguration.License(license);
                                 }

                                 var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
                                 transport.ConnectionString(config.ServiceBusConnectionString);
                                 transport.PrefetchCount(20);
                                 builder.RegisterInstance(transport).As<TransportExtensions<AzureServiceBusTransport>>().SingleInstance();
                                 endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
                                 endpointConfiguration.EnableInstallers();

                                 return endpointConfiguration;
                             })
                   .As<EndpointConfiguration>()
                   .InstancePerLifetimeScope();

            builder.RegisterType<EndpointInstanceFactory>().As<IEndpointInstanceFactory>().InstancePerLifetimeScope();
        }
    }
}