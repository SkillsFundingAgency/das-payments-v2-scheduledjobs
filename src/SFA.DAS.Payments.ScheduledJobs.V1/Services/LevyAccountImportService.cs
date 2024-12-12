using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.FundingSource.Messages.Commands;
using SFA.DAS.Payments.ScheduledJobs.V1.Configuration;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Services
{
    public class LevyAccountImportService : ILevyAccountImportService
    {
        private readonly IAppsettingsOptions _settings;
        private ILogger<LevyAccountImportService> _logger;
        private readonly IEndpointInstanceFactory _endpointInstanceFactory;
        public LevyAccountImportService(IAppsettingsOptions settings
            , ILogger<LevyAccountImportService> logger
            , IEndpointInstanceFactory endpointInstanceFactory)
        {
            _settings = settings;
            _logger = logger;
            this._endpointInstanceFactory = endpointInstanceFactory;
        }

        public async Task RunLevyAccountImport()
        {
            try
            {
                var command = new ImportEmployerAccounts();
                var endpointInstance = await _endpointInstanceFactory.GetEndpointInstance().ConfigureAwait(false);
                await endpointInstance.Send(_settings.Values.LevyAccountBalanceEndpoint, command).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError("Error in LevyAccountImport", e);
                throw;
            }
        }
    }
}
