using Azure.Messaging.EventGrid;

namespace Billy.Function.Extensions;

public static class EventGridEventExtensions
{
    public static bool IsAdvancedMessageReceivedEvent(this EventGridEvent eventGridEvent)
    {
        return eventGridEvent.EventType == "Microsoft.Communication.AdvancedMessageReceived";
    }
}