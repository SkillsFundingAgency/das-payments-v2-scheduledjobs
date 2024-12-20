# SFA.DAS.Payments.ScheduledJobs.V1


## Troubleshoot steps
- npm install -g azure-functions-core-tools@4 --unsafe-perm true
- func start --verbose
## Overview

The `SFA.DAS.Payments.ScheduledJobs.V1` project is an Azure Function application designed to handle scheduled jobs related to the SFA DAS Payments system. This application is responsible for various background tasks such as data cleanup, importing levy account data, and other scheduled maintenance activities.

## Features

- **Data Cleanup**: Periodically cleans up audit data from various queues.
- **Levy Account Import**: Imports levy account data from external sources.
- **Telemetry**: Integrates with Application Insights for logging and telemetry.
- **Database Contexts**: Manages connections to the Payments and Commitments databases.

## Configuration

The application uses `local.settings.json` for configuration. Key settings include:

- **Service Bus Connection Strings**: For connecting to Azure Service Bus.
- **API Credentials**: For accessing external APIs.
- **Database Connection Strings**: For connecting to SQL Server databases.
- **Telemetry Settings**: For Application Insights integration.

## Dependency Injection

The project uses Microsoft Dependency Injection to manage dependencies. Key services and configurations are registered in the `DependencyInjection` class.

### Example Configuration
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
        "LevyAccountValidationSchedule": "0 6 * * *",
        "Version": "1.0",
        "EnvironmentName": "LOCAL",
        "ServiceName": "SFA.DAS.Payments.ScheduledJobs.V1",
        "EstimateSubmissionWindowMetricsSchedule": "0 */15 * * * *",
        "EndpointName": "{endpoint name}",
        "LevyAccountBalanceEndpoint": "{endpoint name}",
        "ApplicationInsightsInstrumentationKey": "",
        "ServiceBusConnectionString": "",
        "DasNServiceBusLicenseKey": "",
        "AccountApiBatchSize": "1000",
        "AccountApiBaseUrl": "http://localhost:9091/",
        "AccountApiClientId": "AccountApiClientId",
        "AccountApiClientSecret": "AccountApiClientSecret",
        "AccountApiIdentifierUri": "http://localhost:9091/",
        "AccountApiTenant": "AccountApiTenant",
        "CurrentCollectionPeriod": "",
        "CurrentAcademicYear": "",
        "PreviousAcademicYearCollectionPeriod": "",
        "PerviousAcademicYear": "",
        "LogLevel": "Information",
        "PreviousAcademicYear": ""
    },
    "ConnectionStrings": {
        "ServiceBusConnectionString": "",
        "CommitmentsConnectionString": "{use connection string from your Payments V2 service bus namespace}",
        "PaymentsConnectionString": "{use connection string from your Payments V2 service bus namespace}"
    }
}




## Implementation

### Key Components

- **AuditDataCleanUpService**: Handles the cleanup of audit data from various queues.
- **LevyAccountImportService**: Manages the import of levy account data.
- **PaymentLogger**: Provides logging functionality.
- **ApplicationInsightsTelemetry**: Integrates with Application Insights for telemetry.
- **PaymentsDataContext**: Manages the connection to the Payments database.
- **CommitmentsDataContext**: Manages the connection to the Commitments database.

### Dependency Injection

The `DependencyInjection` class contains methods to register services and configurations:

- `AddAppsettingsConfiguration`: Registers the application settings.
- `AddPaymentDatabaseContext`: Registers the Payments database context.
- `AddScoppedServices`: Registers scoped services.
- `AddCommitmentsDataContext`: Registers the Commitments database context.

### Example Usage

In the `Startup.cs` or equivalent configuration file, you can configure the services:

public class Startup { public void ConfigureServices(IServiceCollection services) { var configuration = new ConfigurationBuilder() .AddJsonFile("appsettings.json") .Build();
    services.AddSingleton<IConfiguration>(configuration);
    services.AddAppsettingsConfiguration();
    services.AddPaymentDatabaseContext(configuration);
    services.AddScoppedServices();
    services.AddCommitmentsDataContext(configuration);
}
}



## Running the Application

To run the application locally, ensure you have the necessary configuration in `local.settings.json` and use the following command:


This will start the Azure Functions runtime and execute the scheduled jobs as configured.

## Contributing

Contributions are welcome! Please submit a pull request or open an issue to discuss any changes.

## License

This project is licensed under the MIT License.

