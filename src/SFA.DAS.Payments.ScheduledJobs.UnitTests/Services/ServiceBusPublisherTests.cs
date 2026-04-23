using Azure.Messaging.ServiceBus;
using SFA.DAS.Payments.FundingSource.Messages.Commands;
using System.Text.Json;

namespace SFA.DAS.Payments.ScheduledJobs.UnitTests.Services
{
    public class ServiceBusPublisherTests
    {
        [Test]
        public async Task Publisher_sends_message_with_expected_properties()
        {
            // Arrange
            var message = new ImportEmployerAccounts { EventId = Guid.NewGuid() };
            
            var mockSender = new Mock<ServiceBusSender>();
            ServiceBusMessage? publishedMessage = null;

            mockSender
                .Setup(x => x.SendMessageAsync(
                    It.IsAny<ServiceBusMessage>(),
                    It.IsAny<CancellationToken>()))
                .Callback<ServiceBusMessage, CancellationToken>((msg, _) => publishedMessage = msg)
                .Returns(Task.CompletedTask);

            var publisher = new ServiceBusPublisher(mockSender.Object);
            
            // Act
            await publisher.Publish(message);

            // Assert
            mockSender.Verify(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()),
                Times.Once);
            publishedMessage.Should().NotBeNull();
            publishedMessage!.ContentType.Should().Be("application/json");
            publishedMessage.Subject.Should().Be(nameof(ImportEmployerAccounts));
            publishedMessage.MessageId.Should().NotBeNullOrWhiteSpace();
            publishedMessage.ApplicationProperties["messageType"].Should().Be(nameof(ImportEmployerAccounts));
            publishedMessage.ApplicationProperties["NServiceBus.EnclosedMessageTypes"].Should()
                .Be($"{typeof(ImportEmployerAccounts).FullName}, {typeof(ImportEmployerAccounts).Assembly.GetName().Name}");
            var deserializedMessage = JsonSerializer.Deserialize<ImportEmployerAccounts>(publishedMessage.Body.ToString());
            deserializedMessage.Should().NotBeNull();
            deserializedMessage.EventId.Should().Be(message.EventId);
        }
    }
}
