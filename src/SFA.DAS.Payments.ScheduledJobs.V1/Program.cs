using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Payments.ScheduledJobs.V1.IOC;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
        //string currentDirectory = Directory.GetCurrentDirectory();
        //string settingsFilePath = Path.Combine(currentDirectory, "local.settings.json");
        //config.AddJsonFile(settingsFilePath, optional: true, reloadOnChange: true);
    })
     .ConfigureServices((context, services) =>
     {
         services.AddApplicationInsightsTelemetryWorkerService();
         services.ConfigureFunctionsApplicationInsights();

         services.AddAppsettingsConfiguration();
         services.AddScopedServices();
         services.AddSingletonServices();
         services.AddPaymentDatabaseContext(context.Configuration);
         services.AddCommitmentsDataContext(context.Configuration);
         services.AddAccountApiConfiguration(context.Configuration);
         services.AddApplicationLoggerSettings();
         services.ConfigureServiceBusConfiguration(context.Configuration);
     }).Build();

host.Run();