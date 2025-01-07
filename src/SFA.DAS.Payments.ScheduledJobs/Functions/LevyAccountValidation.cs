using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.ScheduledJobs.Services;

namespace SFA.DAS.Payments.ScheduledJobs.Functions
{
    public class LevyAccountValidation
    {
        private readonly ILogger<LevyAccountValidation> _logger;
        private readonly ILevyAccountValidationService _levyAccountValidationService;

        public LevyAccountValidation(ILevyAccountValidationService levyAccountValidationService
            , ILogger<LevyAccountValidation> logger)
        {
            _levyAccountValidationService = levyAccountValidationService;
            _logger = logger;
        }

        [Function("TimerTriggerLevyAccountValidation")]
        public async Task TimerTriggerLevyAccountValidation([TimerTrigger("%LevyAccountValidationSchedule%", RunOnStartup = false)] TimerInfo timerInfo)
        {
            await _levyAccountValidationService.Validate();
        }

        [Function("HttpTriggerLevyAccountValidation")]
        public async Task HttpTriggerLevyAccountValidation([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest httpRequest)
        {
            await _levyAccountValidationService.Validate();
        }
    }
}
