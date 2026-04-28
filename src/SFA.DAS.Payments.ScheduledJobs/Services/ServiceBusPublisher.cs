using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace SFA.DAS.Payments.ScheduledJobs.Services
{
    public class ServiceBusPublisher : IServiceBusPublisher
    {
        private readonly ServiceBusSender _sender;

        public ServiceBusPublisher(ServiceBusSender sender)
        {
            _sender = sender;
        }

        public async Task Publish<T>(T message)
        {
            var messageId = Guid.NewGuid().ToString("N"); // strip dashes

            var json = JsonSerializer.Serialize(message);

            var serviceBusMessage = new ServiceBusMessage(json)
            {
                MessageId = messageId,
                ContentType = "application/json",
                Subject = typeof(T).Name,
                ApplicationProperties =
                {
                    // NServiceBus compatibility headers
                    ["NServiceBus.EnclosedMessageTypes"] = $"{typeof(T).FullName}, {typeof(T).Assembly.GetName().Name}",
                    ["messageType"] = typeof(T).Name
                }
            };

            await _sender.SendMessageAsync(serviceBusMessage);
        }
    }
}
