using Autofac.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.ScheduledJobs.Ioc;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
     {

         var serviceProvider = services.BuildServiceProvider();
         var env = serviceProvider.GetRequiredService<IHostEnvironment>();
         
         services.AddApplicationInsightsTelemetryWorkerService();
         services.ConfigureFunctionsApplicationInsights();


         services.AddPaymentDatabaseContext(context.Configuration, env);
         services.AddCommitmentsDataContext(context.Configuration, env);

         services.AddScopedServices();
         services.AddSingletonServices();

         services.AddAccountApiConfiguration(context.Configuration, env);
         services.AddApplicationLoggerSettings();
         services.ConfigureServiceBusConfiguration(context.Configuration, env);


     }).Build();

host.Run();