using UptimeBot.Console.Core.Worker;

namespace UptimeBot.Console.Infrastructure.Repository;

public interface IWorkerMessageRepository
{
    Task AddAsync(WorkerMessage message);
    Task AddFromContentAsync(string content);

    Task<IEnumerable<WorkerMessage>> GetAsync();

    Task DeleteAsync(int id);
}
