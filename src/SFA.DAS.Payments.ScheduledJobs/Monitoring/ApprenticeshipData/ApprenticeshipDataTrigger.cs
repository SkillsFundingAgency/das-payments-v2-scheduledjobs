using System;
using System.Threading.Tasks;
using AzureFunctions.Autofac;
using Microsoft.AspNetCore.Http;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.ScheduledJobs.Infrastructure.IoC;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;

namespace SFA.DAS.Payments.ScheduledJobs.Monitoring.ApprenticeshipData
{
    [DependencyInjectionConfig(typeof(DependencyInjectionConfig))]
    public class ApprenticeshipDataTrigger
    {
        private readonly ILogger _logger;
        private readonly IApprenticeshipsDataService _service;
        private readonly IPaymentLogger _log;

        public ApprenticeshipDataTrigger(ILogger<ApprenticeshipDataTrigger> logger, IApprenticeshipsDataService service, IPaymentLogger log)
        {
            _logger = logger;
            _service = service;
            _log = log;
        }

        [Function("TimerTriggerApprenticeshipsReferenceDataComparison")]
        public async Task TimerTriggerApprenticeshipsReferenceDataComparison([TimerTrigger("%ApprenticeshipValidationSchedule%", RunOnStartup = false)] TimerInfo myTimer)
        {
            await RunApprenticeshipsReferenceDataComparison(_service, _log);
        }

        [Function("HttpTriggerApprenticeshipsReferenceDataComparison")]
        public async Task HttpTriggerApprenticeshipsReferenceDataComparison([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest httpRequest)
        {
            await RunApprenticeshipsReferenceDataComparison(_service, _log);
        }

        private static async Task RunApprenticeshipsReferenceDataComparison(IApprenticeshipsDataService service, IPaymentLogger log)
        {
            try
            {
                await service.ProcessComparison();
            }
            catch (Exception e)
            {
                log.LogError("Error in ProcessComparison", e);
                throw;
            }
        }
    }
}