using UptimeBot.Console.Core.Worker;

namespace UptimeBot.Console.Infrastructure.Repository;

public class WorkerMessagesFromConf : IWorkerMessageRepository
{
    private List<WorkerMessage> Messages { get; set; }
    private int _messageId;

    public WorkerMessagesFromConf(List<string> urls)
    {
        Messages = urls.Select(url => new WorkerMessage(
                ++_messageId,
                url,
                DateTime.UtcNow,
                WorkerMessageType.Url,
                new TimeSpan(0, 0, 5, 0)
            ))
            .ToList();
    }

    public Task AddAsync(WorkerMessage message)
    {
        throw new NotImplementedException();
    }

    public Task AddFromContentAsync(string content)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<WorkerMessage>> GetAsync()
    {
        lock (Messages)
        {
            var messagesDue = Messages.Where(m => m.DueTime <= DateTime.UtcNow).ToList();
            var updatedMessages = messagesDue
                .Select(m =>
                {
                    if (m.RepeatEvery is null)
                    {
                        return m;
                    }
                    m.DueTime += m.RepeatEvery.Value;
                    return m;
                })
                .ToList();

            return Task.FromResult(updatedMessages.AsEnumerable());
        }
    }
}
