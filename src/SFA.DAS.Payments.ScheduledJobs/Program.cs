using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;


var host = new HostBuilder()
    .ConfigureAppConfiguration(config =>
    {
#if DEBUG
        config.AddJsonFile("local.settings.json", optional: true);
#endif
    })/*.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(builder =>
    {
        builder.RegisterModule(new TelemetryModule());
        builder.RegisterModule(new LoggingModule());
        builder.RegisterModule(new FunctionsModule());
        builder.RegisterModule(new LevyAccountValidationModule());

        builder.RegisterModule(new Modules.PaymentDataContextModule());
        builder.RegisterModule(new CommitmentsDataContextModule());
        builder.RegisterModule(new Modules.ConfigurationModule());
        builder.RegisterModule(new Modules.MessagingModule());
    })*/
    .Build();

host.Run();
