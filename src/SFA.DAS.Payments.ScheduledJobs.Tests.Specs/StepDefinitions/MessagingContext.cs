using NServiceBus;

namespace SFA.DAS.Payments.ScheduledJobs.Tests.Specs.StepDefinitions
{
    public class MessagingContext
    {
        private IEndpointInstance endpointInstance;

        public MessagingContext()
        {
            endpointInstance = TestRunBindings.endpoint;            
        }

        public async Task Send<T>(string messageJson)
        {
            var message = System.Text.Json.JsonSerializer.Deserialize<T>(messageJson);
            await endpointInstance.Send("sfa-das-payments-requiredpayments", message);
        }

        public async Task Send<T>(T earningEvent)
        {
            await endpointInstance.Send("sfa-das-payments-requiredpayments", earningEvent);
        }
    }
}
