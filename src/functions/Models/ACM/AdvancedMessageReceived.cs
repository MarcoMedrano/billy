namespace functions.Models.ACM;

public class AdvancedMessageReceived
{
    public string From { get; set; }
    public string To { get; set; }
    public Guid MessageId { get; set; }
    public string MessageType { get; set; }
    public string Content { get; set; }
    public string ChannelType { get; set; }
    public DateTime ReceivedTimeStamp { get; set; }
}
