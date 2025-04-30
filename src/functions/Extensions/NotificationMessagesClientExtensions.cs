using Azure.Communication.Messages;

namespace Billy.Function.Extensions;

public static class NotificationMessagesClientExtensions
{
    public static string GetMediaUrl(this NotificationMessagesClient client, string mediaId)
    {
        //https://github.com/Azure/azure-sdk-for-net/blob/2bd52ba3afc188f63f7baec2a3eabd402812d4af/sdk/communication/Azure.Communication.Messages/src/Generated/NotificationMessagesClient.cs#L18
        // var uri = new RawRequestUriBuilder();
        //     uri.Reset(_endpoint);
        //     uri.AppendPath("/messages/streams/", false);
        //     uri.AppendPath(id, true);
        //     uri.AppendQuery("api-version", _apiVersion, true);
        //     request.Headers.Add("Accept", "application/octet-stream");

        // return $"{endpoint}/messages/streams/{mediaId}?api-version={_apiVersion}";
        return string.Empty;
    }
}