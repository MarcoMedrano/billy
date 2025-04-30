// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using System.Text.Json;
using Azure;
using Azure.Communication.Messages;
using Azure.Messaging.EventGrid;
using Billy.Function.Extensions;
using Billy.Function.Models.ACM;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Billy.Function;

public class MessageReceived(ILogger<MessageReceived> _logger, NotificationMessagesClient _notificationMessagesClient, JsonSerializerOptions _jsonSerializerOptions)
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
                    await DownloadMediaWithStreamAsync(message.Media.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process the EventGrid event.");
        }
    }

    public async Task DownloadMediaWithStreamAsync(Guid mediaId)
    {
        Response<Stream> fileResponse;
        try
        {
            // Download media to stream
            fileResponse = await _notificationMessagesClient.DownloadMediaAsync(mediaId.ToString());

            Console.WriteLine(fileResponse.ToString());
        }
        catch (RequestFailedException e)
        {
            _logger.LogError(e, "Failed to download media with ID {mediaId}", mediaId);
            return;
        }

        var contentType = fileResponse.GetRawResponse().Headers.ContentType;
        string fileExtension = GetFileExtension(contentType);

        _logger.LogInformation("File extension: {fileExtension}", fileExtension);

        // Write the media stream to the file
        // using (Stream outStream = File.OpenWrite(filePath))
        // {
        //     fileResponse.Value.CopyTo(outStream);
        // }
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
