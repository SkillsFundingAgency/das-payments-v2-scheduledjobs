
namespace SFA.DAS.Payments.ScheduledJobs.Services
{
    public interface IServiceBusPublisher
    {
        Task Publish<T>(T message);
    }
}
