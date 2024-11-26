using UptimeBot.Console.Core.Worker;

namespace UptimeBot.Console.Infrastructure.Repository;

public class WorkerMessagesTemp : IWorkerMessageRepository
{
    private List<WorkerMessage> Messages { get; set; } = [];
    private int _messageId;

    public Task AddAsync(WorkerMessage message)
    {
        System.Console.WriteLine($"[WorkerMessagesTemp] AddAsync: {message}");

        lock (Messages)
        {
            Messages.Add(message);
        }
        return Task.CompletedTask;
    }

    public Task AddFromContentAsync(string content)
    {
        lock (Messages)
        {
            System.Console.WriteLine($"[WorkerMessagesTemp] AddFromContentAsync: {content}");
            WorkerMessage? message = null;

            if (content.StartsWith("http") && !content.Contains(' '))
            {
                message = new WorkerMessage(
                    ++_messageId,
                    content,
                    DateTime.UtcNow,
                    WorkerMessageType.Url,
                    new TimeSpan(0, 0, 1, 0)
                );
                Messages.Add(message);
            }
            else
            {
                message = new WorkerMessage(
                    ++_messageId,
                    content,
                    DateTime.UtcNow,
                    WorkerMessageType.Message
                );
                Messages.Add(message);
            }
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        lock (Messages)
        {
            Messages.RemoveAll(m => m.Id == id);
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<WorkerMessage>> GetAsync()
    {
        lock (Messages)
        {
            var messageIds = Messages.Select(m => m.Id);
            if (messageIds.Any())
            {
                System.Console.WriteLine(
                    $"[WorkerMessagesTemp] Found {messageIds.Count()} messages. Removing..."
                );
                Messages.RemoveAll(m =>
                    m.Type == WorkerMessageType.Message && messageIds.Contains(m.Id)
                );
            }
            return Task.FromResult(Messages.AsEnumerable());
        }
    }
}
