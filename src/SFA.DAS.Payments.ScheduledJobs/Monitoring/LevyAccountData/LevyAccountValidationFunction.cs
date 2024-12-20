using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;

namespace SFA.DAS.Payments.ScheduledJobs.Monitoring.LevyAccountData
{
    public class LevyAccountValidationFunction
    {
        private readonly ILevyAccountValidationService _levyAccountValidationService;

        public LevyAccountValidationFunction(ILevyAccountValidationService levyAccountValidationService)
        {
            _levyAccountValidationService = levyAccountValidationService;
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
