using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using Billy.Function.Extensions;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services.AddNotificationMessagesClient();
builder.Services.AddAzureContentUnderstandingClient();
builder.Services.AddAzureOpenAIClient();
builder.Services.AddJsonSerializerOptions();

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
