using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Payments.ScheduledJobs.Functions
{
    public class ApprenticeshipDataTrigger
    {
        private readonly ILogger<ApprenticeshipDataTrigger> _logger;
        private readonly IApprenticeshipDataService _apprenticeshipDataServicee;

        public ApprenticeshipDataTrigger(ILogger<ApprenticeshipDataTrigger> logger
            , IApprenticeshipDataService service)
        {
            _logger = logger;
            _apprenticeshipDataServicee = service;
        }

        [Function("TimerTriggerApprenticeshipsReferenceDataComparison")]
        public async Task TimerTriggerApprenticeshipsReferenceDataComparison([TimerTrigger("%ApprenticeshipValidationSchedule%", RunOnStartup = false)] TimerInfo myTimer)
        {
            await _apprenticeshipDataServicee.ProcessComparison();
        }

        [Function("HttpTriggerApprenticeshipsReferenceDataComparison")]
        public async Task HttpTriggerApprenticeshipsReferenceDataComparison([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest httpRequest)
        {
            await _apprenticeshipDataServicee.ProcessComparison();
        }
    }
}
