using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.FundingSource.Messages.Commands;

namespace SFA.DAS.Payments.ScheduledJobs.Services
{
    public class LevyAccountImportService : ILevyAccountImportService
    {
        private IServiceBusPublisher _serviceBusPublisher;
        private ILogger<LevyAccountImportService> _logger;
        public LevyAccountImportService(IServiceBusPublisher serviceBusPublisher, ILogger<LevyAccountImportService> logger)
        {
            _serviceBusPublisher = serviceBusPublisher;
            _logger = logger;
        }

        public async Task<ImportEmployerAccounts> RunLevyAccountImport()
        {
            var message = new ImportEmployerAccounts();
            try
            {
                await _serviceBusPublisher.Publish(message);
                _logger.LogInformation($"Published ImportEmployerAccounts EventId {message.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to publish ImportEmployerAccounts EventId {message.EventId}");
                throw;
            }

            return message;
        }
    }
}
