using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Payments.ScheduledJobs.IOC;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
       
    })
     .ConfigureServices((context, services) =>
     {
         services.AddApplicationInsightsTelemetryWorkerService();
         services.ConfigureFunctionsApplicationInsights();


         services.AddPaymentDatabaseContext(context.Configuration);
         services.AddCommitmentsDataContext(context.Configuration);

         services.AddAppsettingsConfiguration();
         services.AddScopedServices();
         services.AddSingletonServices();

         services.AddAccountApiConfiguration(context.Configuration);
         services.AddApplicationLoggerSettings();
         services.ConfigureServiceBusConfiguration(context.Configuration);
     }).Build();

host.Run();