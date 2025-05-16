using Azure.Messaging.ServiceBus;

namespace SFA.DAS.Payments.ScheduledJobs.ServiceBus
{
    public interface IServiceBusClientHelper
    {
        Task ReceiveMessagesFromQueueAsync(string queueName, Func<ServiceBusReceivedMessage, Task> processMessage, int maxConcurrentCalls = 1);
        Task ReceiveMessagesFromSubscriptionAsync(string topicName, string subscriptionName, Func<ServiceBusReceivedMessage, Task> processMessage, int maxConcurrentCalls = 1);
        Task SendMessageToQueueAsync(string queueName, string message);
        Task SendMessageToTopicAsync(string topicName, string message);
    }
}