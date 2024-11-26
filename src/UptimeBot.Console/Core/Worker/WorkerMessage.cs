namespace UptimeBot.Console.Core.Worker;

public enum WorkerMessageType
{
    Url,
    Message,
}

public class WorkerMessage
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime DueTime { get; set; }
    public WorkerMessageType Type { get; set; }
    public TimeSpan? RepeatEvery { get; set; }

    public WorkerMessage(
        int id,
        string content,
        DateTime dueTime,
        WorkerMessageType type,
        TimeSpan? repeatEvery = null
    )
    {
        Id = id;
        Content = content;
        DueTime = dueTime;
        Type = type;
        RepeatEvery = repeatEvery;
    }

    public override string ToString()
    {
        return $"Id: {Id}, Content: {Content}, DueTime: {DueTime}, Type: {Type}, RepeatEvery: {RepeatEvery}";
    }
}
