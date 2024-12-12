using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.ScheduledJobs.V1.Services;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Functions
{
    public class LevyAccountImport
    {
        private readonly ILogger<LevyAccountImport> _logger;
        private readonly ILevyAccountImportService _levyAccountImportService;

        public LevyAccountImport(ILogger<LevyAccountImport> logger
            , ILevyAccountImportService levyAccountImportService)
        {
            _logger = logger;
            _levyAccountImportService = levyAccountImportService;
        }

        [Function("LevyAccountImport")]
        public async Task Run([TimerTrigger("%LevyAccountSchedule%")] TimerInfo myTimer)
        {
            await _levyAccountImportService.RunLevyAccountImport();
        }

        [Function("HttpLevyAccountImport")]
        public async Task HttpLevyAccountImport([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            await _levyAccountImportService.RunLevyAccountImport();
        }
    }
}
