using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.ScheduledJobs.V1.Configuration;

namespace SFA.DAS.Payments.ScheduledJobs.V1
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;
        private readonly IAppsettingsOptions _options;
        private readonly IConfiguration _configuration;
        public Function1(ILogger<Function1> logger
            , IAppsettingsOptions options,
                IConfiguration configuration)
        {
            _logger = logger;
            _options = options;
            _configuration = configuration;
        }

        [Function("Function1")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            string message = _configuration["Values:DataLockAuditDataCleanUpQueue"];

            var vr = _options.Values;
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
