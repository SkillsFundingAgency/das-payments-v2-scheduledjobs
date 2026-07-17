using System.Collections.Concurrent;
using NServiceBus;
using SFA.DAS.Payments.ScheduledJobs.Bindings;

namespace SFA.DAS.Payments.ScheduledJobs.Tests.Specs.Handlers
{
    public class AuditDataCleanupHandler : IHandleMessages<AuditDataCleanUpBinding>
    {
        public static ConcurrentBag<AuditDataCleanUpBinding> ReceivedEvents { get; } = new ConcurrentBag<AuditDataCleanUpBinding>();
        public Task Handle(AuditDataCleanUpBinding message, IMessageHandlerContext context)
        {
            ReceivedEvents.Add(message);
            return Task.CompletedTask;
        }

        public static IEnumerable<AuditDataCleanUpBinding> GetEvents() => ReceivedEvents.ToList();
    }
}
