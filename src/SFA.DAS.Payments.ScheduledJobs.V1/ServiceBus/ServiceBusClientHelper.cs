using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace SFA.DAS.Payments.ScheduledJobs.V1.ServiceBus
{
    public class ServiceBusClientHelper : IServiceBusClientHelper
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ILogger<ServiceBusClientHelper> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public ServiceBusClientHelper(ServiceBusClient client, ILogger<ServiceBusClientHelper> logger)
        {
            _serviceBusClient = client;
            _logger = logger;
            _retryPolicy = Policy.Handle<Exception>()
                                 .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                                    (exception, timeSpan, retryCount, context) =>
                                                    {
                                                        _logger.LogWarning($"Retry {retryCount} encountered an error: {exception.Message}. Waiting {timeSpan} before next retry.");
                                                    });
        }

        public async Task SendMessageToQueueAsync(string queueName, string message)
        {
            var sender = _serviceBusClient.CreateSender(queueName);
            var serviceBusMessage = new ServiceBusMessage(message);

            await _retryPolicy.ExecuteAsync(async () =>
            {
                await sender.SendMessageAsync(serviceBusMessage);
            });
        }

        public async Task SendMessageToTopicAsync(string topicName, string message)
        {
            var sender = _serviceBusClient.CreateSender(topicName);
            var serviceBusMessage = new ServiceBusMessage(message);

            await _retryPolicy.ExecuteAsync(async () =>
            {
                await sender.SendMessageAsync(serviceBusMessage);
            });
        }

        public async Task ReceiveMessagesFromQueueAsync(string queueName, Func<ServiceBusReceivedMessage, Task> processMessage, int maxConcurrentCalls = 1)
        {
            var processorOptions = new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = maxConcurrentCalls,
                AutoCompleteMessages = false
            };

            var processor = _serviceBusClient.CreateProcessor(queueName, processorOptions);

            processor.ProcessMessageAsync += async args =>
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await processMessage(args.Message);
                    await args.CompleteMessageAsync(args.Message);
                });
            };

            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception, "Message handler encountered an exception");
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync();
        }

        public async Task ReceiveMessagesFromSubscriptionAsync(string topicName, string subscriptionName, Func<ServiceBusReceivedMessage, Task> processMessage, int maxConcurrentCalls = 1)
        {
            var processorOptions = new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = maxConcurrentCalls,
                AutoCompleteMessages = false
            };

            var processor = _serviceBusClient.CreateProcessor(topicName, subscriptionName, processorOptions);

            processor.ProcessMessageAsync += async args =>
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await processMessage(args.Message);
                    await args.CompleteMessageAsync(args.Message);
                });
            };

            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception, "Message handler encountered an exception");
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync();
        }
    }
}
