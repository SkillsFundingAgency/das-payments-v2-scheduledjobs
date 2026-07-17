using Microsoft.Extensions.Configuration;
using NServiceBus;
using SFA.DAS.Payments.Messages.Common;

namespace SFA.DAS.Payments.ScheduledJobs.Tests.Specs.StepDefinitions
{
    [Binding]
    public class TestRunBindings 
    {
        public static IEndpointInstance endpoint { get; private set; }
        public static IConfiguration Config { get; private set; }

        [BeforeTestRun]
        public static async Task SetUpMessaging()
        {
            Config = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appSettings.json"))
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appSettings.local.json"), true)
                .Build();

            var endpointConfig = new EndpointConfiguration("sfa-das-payments-requiredpayments-tests-specs");
            var conventions = endpointConfig.Conventions();
            conventions.DefiningMessagesAs(type => type.IsMessage());
            endpointConfig.UseSerialization<NewtonsoftJsonSerializer>();
            //endpointConfig.SendOnly();
            var storageConnectionString = Config["ConnectionStrings:StorageConnectionString"];
            endpointConfig.UsePersistence<AzureTablePersistence>().ConnectionString(storageConnectionString);
            endpointConfig.UseTransport<AzureServiceBusTransport>()
                .ConnectionString(Config["ConnectionStrings:ServiceBusConnectionString"]);
            endpointConfig.EnableInstallers();
            var startable = await Endpoint.Create(endpointConfig);
            endpoint = await startable.Start();
        }
    }
}
