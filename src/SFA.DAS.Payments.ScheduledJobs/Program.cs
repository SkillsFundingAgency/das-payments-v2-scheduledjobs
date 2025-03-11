using Autofac.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Payments.ScheduledJobs.Ioc;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {

    })
     .ConfigureServices((context, services) =>
     {
         services.AddApplicationInsightsTelemetryWorkerService();
         services.ConfigureFunctionsApplicationInsights();


         services.AddPaymentDatabaseContext(context.Configuration);
         services.AddCommitmentsDataContext(context.Configuration);

         services.AddAppSettingsConfiguration();
         services.AddScopedServices();
         services.AddSingletonServices();

         services.AddAccountApiConfiguration(context.Configuration);
         services.AddApplicationLoggerSettings();
         services.ConfigureServiceBusConfiguration(context.Configuration);
     }).Build();

host.Run();