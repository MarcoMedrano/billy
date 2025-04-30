// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using System.Text.Json;
using Azure;
using Azure.Communication.Messages;
using Azure.Messaging.EventGrid;
using Billy.Function.AzureContentUnderstanding;
using Billy.Function.Extensions;
using Billy.Function.Models;
using Billy.Function.Models.ACM;
using Billy.Function.Parsing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Billy.Function;

public class MessageReceived(
    ILogger<MessageReceived> _logger, 
    NotificationMessagesClient _notificationMessagesClient, 
    AzureContentUnderstandingClient _azureContentUnderstandingClient,
    JsonSerializerOptions _jsonSerializerOptions)
{
    private static Guid _channelRegistrationId = Guid.Parse("37fe8c93-1377-4153-99a1-a58995a34f36");

    [Function(nameof(MessageReceived))]
    public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
    {
        _logger.LogInformation("C# EventGrid trigger function called, processing event...");
        try
        {
            //_logger.LogInformation("Event type: {type}, Event subject: {subject}", eventGridEvent.EventType, eventGridEvent.Subject);

            if (eventGridEvent.IsAdvancedMessageReceivedEvent())
            {
                _logger.LogInformation("Deserializing data " + eventGridEvent.Data.ToString());

                var message = JsonSerializer.Deserialize<AdvancedMessageReceived>(eventGridEvent.Data.ToString(), _jsonSerializerOptions);
                _logger.LogInformation("Message type: {messageType}", message.MessageType);

                if (message.Media != null)
                {
                    _logger.LogInformation("Media received: mimeType: {mimeType}, id: {id}, animated: {animated}",
                        message.Media.MimeType, message.Media.Id, message.Media.Animated);
                    await SendWhatsAppMessageAsync(message.From, "Received media with ID: " + message.Media.Id);
                    var filePath = await DownloadMediaWithStreamAsync(message.Media.Id);
                    var responseMessage = await _azureContentUnderstandingClient.BeginAnalyzeAsync("BillAnalyzer", filePath);
                    _logger.LogInformation("Response message: {responseMessage}", responseMessage.ToString());
                    var result = await _azureContentUnderstandingClient.PollResultAsync(responseMessage);
                    var json = _azureContentUnderstandingClient.GetJsonFields(result);
                    Invoice invoice  = InvoiceParser.Parse(json);
                    _logger.LogInformation($"Invoice Details:");
                    _logger.LogInformation($"Customer: {invoice.CustomerName}");
                    _logger.LogInformation($"Amount Due: {invoice.AmountDue}");
                    _logger.LogInformation($"Invoice Date: {invoice.InvoiceDate:yyyy-MM-dd}");
                    _logger.LogInformation($"Due Date: {invoice.DueDate:yyyy-MM-dd}");
                    _logger.LogInformation($"Total Items: {invoice.Items?.Count ?? 0}");
                    
                    // Display item details if available
                    if (invoice.Items != null && invoice.Items.Count > 0)
                    {
                        _logger.LogInformation("\nItem Details:");
                        foreach (var item in invoice.Items)
                        {
                            _logger.LogInformation($"- {item.Description}: {item.TotalPrice}");
                        }
                    }
                    // File.Delete(filePath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process the EventGrid event.");
        }
    }

    public async Task<string> DownloadMediaWithStreamAsync(Guid mediaId)
    {
        Response<Stream> fileResponse;
        try
        {
            // Download media to stream

            fileResponse = await _notificationMessagesClient.DownloadMediaAsync(mediaId.ToString());

            _logger.LogInformation(fileResponse.ToString());
        }
        catch (RequestFailedException e)
        {
            _logger.LogError(e, "Failed to download media with ID {mediaId}", mediaId);
            throw;
        }

        var contentType = fileResponse.GetRawResponse().Headers.ContentType;
        string fileExtension = GetFileExtension(contentType);

        _logger.LogInformation("File extension: {fileExtension}", fileExtension);
        string filePath = Path.Combine(Path.GetTempPath(), $"{mediaId}.{fileExtension}");
        // Write the media stream to the file
        using Stream outStream = File.OpenWrite(filePath);
        fileResponse.Value.CopyTo(outStream);

        return filePath;
    }

    private async Task SendWhatsAppMessageAsync(string numberToRespondTo, string message)
    {
        var textContent = new TextNotificationContent(_channelRegistrationId, [numberToRespondTo], message);
        await _notificationMessagesClient.SendAsync(textContent);
    }

    private static string GetFileExtension(string contentType)
    {
        return MimeTypes.TryGetValue(contentType, out var extension) ? extension : string.Empty;
    }

    private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>
        {
            { "application/pdf", ".pdf" },
            { "image/jpeg", ".jpg" },
            { "image/png", ".png" },
            { "video/mp4", ".mp4" },
        };
}
