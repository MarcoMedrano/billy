using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Communication.Messages;

var builder = FunctionsApplication.CreateBuilder(args);

// Configure NotificationMessagesClient with a connection string from environment variables
builder.Services.AddSingleton(provider =>
{
    var connectionString = Environment.GetEnvironmentVariable("ACS_CONNECTIONSTRING");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Environment variable 'ACS_CONNECTIONSTRING' is not set.");
    }

    return new NotificationMessagesClient(connectionString);
});

// Configure JsonSerializerOptions
builder.Services.AddSingleton(provider =>
{
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    options.Converters.Add(new JsonStringEnumConverter());
    return options;
});

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
