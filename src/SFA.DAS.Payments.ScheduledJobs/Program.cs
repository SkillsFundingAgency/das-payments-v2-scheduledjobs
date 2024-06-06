using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureAppConfiguration(config =>
    {
#if DEBUG
        config.AddJsonFile("local.settings.json", optional: true);
#endif
    })
    .ConfigureFunctionsWebApplication()
    .Build();

host.Run();
