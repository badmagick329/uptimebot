namespace UptimeBot.Console.Core.Worker;

public class NotificationMessage
{
    public ulong ChannelId { get; set; }
    public string Content { get; set; }
    public bool IsDm { get; set; }

    public NotificationMessage(ulong channelId, string content, bool isDm)
    {
        ChannelId = channelId;
        Content = content;
        IsDm = isDm;
    }

    public override string ToString()
    {
        return $"ChannelId: {ChannelId}, Content: {Content}, IsDm: {IsDm}";
    }
}
