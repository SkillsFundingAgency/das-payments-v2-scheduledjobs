using Autofac.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.ScheduledJobs.Ioc;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
     {
         // Log the current environment
         var serviceProvider = services.BuildServiceProvider();
         var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
         var env = serviceProvider.GetRequiredService<IHostEnvironment>();
         logger.LogInformation($"Current environment: {env.EnvironmentName}");


         services.AddApplicationInsightsTelemetryWorkerService();
         services.ConfigureFunctionsApplicationInsights();


         services.AddPaymentDatabaseContext(context.Configuration, env);
         services.AddCommitmentsDataContext(context.Configuration, env);

         services.AddAppSettingsConfiguration(env);
         services.AddScopedServices();
         services.AddSingletonServices();

         services.AddAccountApiConfiguration(context.Configuration, env);
         services.AddApplicationLoggerSettings();
         services.ConfigureServiceBusConfiguration(context.Configuration, env);


     }).Build();

host.Run();