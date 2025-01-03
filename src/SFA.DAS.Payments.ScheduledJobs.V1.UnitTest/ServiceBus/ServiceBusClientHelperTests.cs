using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Moq;
using Polly;
using Polly.Retry;
using SFA.DAS.Payments.ScheduledJobs.V1.ServiceBus;

namespace SFA.DAS.Payments.ScheduledJobs.V1.UnitTest.ServiceBus
{
    [TestFixture]
    public class ServiceBusClientHelperTests
    {
        private Mock<ServiceBusClient> _mockServiceBusClient;
        private Mock<ServiceBusSender> _mockServiceBusSender;
        private Mock<ServiceBusProcessor> _mockServiceBusProcessor;
        private Mock<ServiceBusReceivedMessage> _mockServiceBusReceivedMessage;
        private Mock<ILogger<ServiceBusClientHelper>> _mockLogger;
        private AsyncRetryPolicy _retryPolicy;
        private ServiceBusClientHelper _serviceBusClientHelper;

        [SetUp]
        public void SetUp()
        {
            _mockServiceBusClient = new Mock<ServiceBusClient>();
            _mockServiceBusSender = new Mock<ServiceBusSender>();
            _mockServiceBusProcessor = new Mock<ServiceBusProcessor>();
            _mockServiceBusReceivedMessage = new Mock<ServiceBusReceivedMessage>();
            _mockLogger = new Mock<ILogger<ServiceBusClientHelper>>();

            _retryPolicy = Policy.Handle<Exception>().RetryAsync(3);

            _mockServiceBusClient
                .Setup(client => client.CreateSender(It.IsAny<string>()))
                .Returns(_mockServiceBusSender.Object);

            _mockServiceBusClient
                .Setup(client => client.CreateProcessor(It.IsAny<string>(), It.IsAny<ServiceBusProcessorOptions>()))
                .Returns(_mockServiceBusProcessor.Object);

            _serviceBusClientHelper = new ServiceBusClientHelper(
                _mockServiceBusClient.Object,
                _mockLogger.Object,
                _retryPolicy
            );
        }

        [Test]
        public async Task SendMessageToQueueAsync_ShouldSendMessage()
        {
            // Arrange
            var queueName = "test-queue";
            var message = "test-message";

            // Act
            await _serviceBusClientHelper.SendMessageToQueueAsync(queueName, message);

            // Assert
            _mockServiceBusSender.Verify(sender => sender.SendMessageAsync(It.Is<ServiceBusMessage>(msg => msg.Body.ToString() == message), CancellationToken.None), Times.Once);
        }
    }
}
