using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.ScheduledJobs.Services;

namespace SFA.DAS.Payments.ScheduledJobs.Functions;

public class GsoAuditDataCleanUpTrigger
{
    private readonly ILogger _logger;
    private readonly IGsoAuditDataCleanUpService _gsoAuditDataCleanUpService;

    public GsoAuditDataCleanUpTrigger(ILoggerFactory loggerFactory, IGsoAuditDataCleanUpService gsoAuditDataCleanUpService)
    {
        _logger = loggerFactory.CreateLogger<GsoAuditDataCleanUpTrigger>();
        _gsoAuditDataCleanUpService = gsoAuditDataCleanUpService;
    }

    [Function("GSOAuditDataCleanUpTimerTrigger")]
    public async Task GsoAuditDataCleanUpTimerTrigger([TimerTrigger("%GsoAuditDataCleanUpSchedule%")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        if (myTimer.ScheduleStatus is not null)
        {
            await _gsoAuditDataCleanUpService.CleanUpGsoAuditData();
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }

    [Function("GSOAuditDataCleanUpHttpTrigger")]
    public async Task GsoAuditDataCleanUpHttpTrigger(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GSOAuditDataCleanUpHttpTrigger")] HttpRequest httpRequest)
    {
        var response = httpRequest.HttpContext.Response;
        try
        {
            await _gsoAuditDataCleanUpService.CleanUpGsoAuditData();

            response.StatusCode = (int)HttpStatusCode.OK;
            await response.WriteAsync("Request processed successfully");
        }
        catch (Exception ex)
        {
            string errorMessage = $"An error occurred while processing the request. {ex.Message}";
            _logger.LogError(errorMessage);
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await response.WriteAsync(errorMessage);
        }
    }
}
