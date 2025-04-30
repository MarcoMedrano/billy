// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}


using System.Text.Json;
using Azure.Communication.Messages;
using Azure.Messaging.EventGrid;
using functions.Models.ACM;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Billy.Function.Extensions;

namespace Billy.Function;

public class MessageReceived
{
    private readonly ILogger<MessageReceived> _logger;

    public MessageReceived(ILogger<MessageReceived> logger)
    {
        _logger = logger;
    }

    [Function(nameof(MessageReceived))]
    public void Run([EventGridTrigger] EventGridEvent eventGridEvent)
    {
        _logger.LogInformation("C# EventGrid trigger function called, processing event...");
        try
        {
            _logger.LogInformation("Event type: {type}, Event subject: {subject}", eventGridEvent.EventType, eventGridEvent.Subject);

            if (eventGridEvent.IsAdvancedMessageReceivedEvent())
            {
                _logger.LogInformation("Deserializing data " + eventGridEvent.Data.ToString());
                var messageData = JsonSerializer.Deserialize<AdvancedMessageReceived>(eventGridEvent.Data.ToString(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                _logger.LogInformation("Message received from: {from}, to: {to}, messageId: {messageId}, messageType: {messageType}, content: {content}, channelType: {channelType}, receivedTimeStamp: {receivedTimeStamp}",
                    messageData.From, messageData.To, messageData.MessageId, messageData.MessageType, messageData.Content, messageData.ChannelType, messageData.ReceivedTimeStamp);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process the EventGrid event.");
        }
    }
}
