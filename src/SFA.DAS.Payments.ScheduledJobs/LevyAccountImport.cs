using System;
using System.Threading.Tasks;
using AzureFunctions.Autofac;
using Microsoft.AspNetCore.Http;
using NServiceBus;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.FundingSource.Messages.Commands;
using SFA.DAS.Payments.ScheduledJobs.Infrastructure.Configuration;
using SFA.DAS.Payments.ScheduledJobs.Infrastructure.IoC;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;

// ReSharper disable UnusedMember.Global

namespace SFA.DAS.Payments.ScheduledJobs
{
    [DependencyInjectionConfig(typeof(DependencyInjectionConfig))]
    public class LevyAccountImport
    {
        private readonly ILogger _logger;
        private readonly IEndpointInstanceFactory _endpointInstanceFactory;
        private readonly IScheduledJobsConfiguration _config;
        private readonly IPaymentLogger _log;

        public LevyAccountImport(ILogger<LevyAccountImport> logger, IEndpointInstanceFactory endpointInstanceFactory, IScheduledJobsConfiguration config, IPaymentLogger log)
        {
            _logger = logger;
            _endpointInstanceFactory = endpointInstanceFactory;
            _config = config;
            _log = log;
        }

        [Function("LevyAccountImport")]
        public async Task Run([TimerTrigger("%LevyAccountSchedule%", RunOnStartup = false)] TimerInfo myTimer)
        {
            await RunLevyAccountImport(_endpointInstanceFactory, _config, _log);
        }

        [Function("HttpLevyAccountImport")]
        public async Task HttpLevyAccountImport([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            await RunLevyAccountImport(_endpointInstanceFactory, _config, _log);
        }

        private static async Task RunLevyAccountImport(IEndpointInstanceFactory endpointInstanceFactory, IScheduledJobsConfiguration config, IPaymentLogger log)
        {
            try
            {
                var command = new ImportEmployerAccounts();
                var endpointInstance = await endpointInstanceFactory.GetEndpointInstance().ConfigureAwait(false);
                await endpointInstance.Send(config.LevyAccountBalanceEndpoint, command).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                log.LogError("Error in LevyAccountImport", e);
                throw;
            }
        }
    }
}
