using System.Threading.Channels;
using UptimeBot.Console.Infrastructure.Repository;

namespace UptimeBot.Console.Core.Worker;

public class Worker
{
    private readonly CancellationTokenSource _cts;
    private readonly List<Task> _tasks = [];
    private readonly Publisher _publisher;
    private readonly List<Subscriber> _subscribers;

    public Worker(IWorkerMessageRepository workerMessageRepository)
    {
        System.Console.WriteLine("[Worker] starting");
        _cts = new CancellationTokenSource();
        var channel = Channel.CreateUnbounded<WorkerMessage>();
        _publisher = new Publisher(channel.Writer, workerMessageRepository, _cts.Token);
        _subscribers = Enumerable
            .Range(1, 3)
            .Select(id => new Subscriber(channel.Reader, id, _cts.Token))
            .ToList();
    }

    public void AddNotificationHandler(Func<string, Task> notificationHandler)
    {
        _subscribers.ForEach(s => s.AddNotificationHandler(notificationHandler));
    }

    public void Start()
    {
        _tasks.Add(_publisher.StartAsync());
        _tasks.AddRange(_subscribers.Select(s => s.StartAsync()));
        System.Console.WriteLine("[Worker] started");
    }

    public async Task Shutdown()
    {
        _cts.Cancel();
        System.Console.WriteLine("[Worker] Shutting down...");
        await Task.WhenAll(_tasks);
        System.Console.WriteLine("[Worker] Shutdown complete.");
    }
}
