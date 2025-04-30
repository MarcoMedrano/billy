using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Communication.Messages;
using Billy.Function.AzureContentUnderstanding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Billy.Function.Extensions;

public static class ServiceCollectionExtensions
{
    // Configure NotificationMessagesClient with a connection string from environment variables
    public static IServiceCollection AddNotificationMessagesClient(this IServiceCollection services)
    {
        services.AddSingleton(provider =>
        {
            var connectionString = Environment.GetEnvironmentVariable("ACS_CONNECTIONSTRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Environment variable 'ACS_CONNECTIONSTRING' is not set.");
            }

            return new NotificationMessagesClient(connectionString);
        });

        return services;
    }

    public static IServiceCollection AddAzureContentUnderstandingClient(this IServiceCollection services)
    {
        services.AddSingleton(provider =>
        {
            var endpoint = "https://billyaiservices783743165220.cognitiveservices.azure.com";
            var apiVersion = "2024-12-01-preview";
            var subscriptionKey = Environment.GetEnvironmentVariable("ACU_SUBSCRIPTION_KEY");
            var apiToken = Environment.GetEnvironmentVariable("ACU_API_TOKEN");

            var logger = provider.GetRequiredService<ILogger<AzureContentUnderstandingClient>>();
            return new AzureContentUnderstandingClient(logger, endpoint, apiVersion, subscriptionKey, apiToken);
        });

        return services;

    }

    public static IServiceCollection AddJsonSerializerOptions(this IServiceCollection services)
    {
        services.AddSingleton(provider =>
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        });

        return services;
    }
}