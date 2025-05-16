# Payments V2 Scheduled Jobs

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://dev.azure.com/sfa-gov-uk/DCT/_apis/build/status/GitHub/Service%20Fabric/SkillsFundingAgency.das-payments-v2-scheduledjobs?branchName=main)](https://dev.azure.com/sfa-gov-uk/DCT/_apis/build/status/GitHub/Service%20Fabric/SkillsFundingAgency.das-payments-v2-scheduledjobs?branchName=main)
[![Jira Project](https://img.shields.io/badge/Jira-Project-blue)](https://skillsfundingagency.atlassian.net/secure/RapidBoard.jspa?rapidView=782&projectKey=PV2)
[![Confluence Project](https://img.shields.io/badge/Confluence-Project-blue)](https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/3700621400/Provider+and+Employer+Payments+Payments+BAU)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)


## How It Works

The Payments V2 Schedule Jobs repo contains an Azure Function and associated function around controlling Payments V2 scheduled jobs.

More information here: 
- https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/400130049/4.+Payments+v2+-+Components+DAS+Space
- https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/533856326/a.+Apprenticeship+Earning+Event+DAS+Space

## 🚀 Installation

### Pre-Requisites

Setup instructions can be found at the following link, which will help you set up your environment and access the correct repositories: https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/950927878/Development+Environment+-+Payments+V2+DAS+Space

### Config

To run locally, create a file `local.settings.json` in the `SFA.DAS.Payments.ScheduledJobs` project, containing the following:

```
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "DataLockAuditDataCleanUpQueue": "{use queue name from your Payments V2 service bus namespace}",
    "EarningAuditDataCleanUpQueue": "{use queue name from your Payments V2 service bus namespace}",
    "FundingSourceAuditDataCleanUpQueue": "{use queue name from your Payments V2 service bus namespace}",
    "LevyAccountSchedule": "0 6 * * *",
    "RequiredPaymentAuditDataCleanUpQueue": "{use queue name from your Payments V2 service bus namespace}",
    "ApprenticeshipValidationSchedule": "0 6 * * *",
    "AuditDataCleanUpSchedule": "0 6 * * *",
    "LevyAccountValidationSchedule": "0 6 * * *"
  },
  "ConnectionStrings": {
    "ServiceBusConnectionString": "{use connection string from your Payments V2 service bus namespace}"
  }
}
```

The CRON settings in the above configuration are just examples, to change the frequency of the jobs to a time other than at 6am daily, refer to the website https://crontab.guru/ for the relevant CRON syntax.

## 🔗 External Dependencies

N/A

## Technologies

* .NetCore 2.1/3.1/6
* Azure SQL Server
* Azure Functions
* Azure Service Bus
* ServiceFabric

## 🐛 Known Issues

N/A