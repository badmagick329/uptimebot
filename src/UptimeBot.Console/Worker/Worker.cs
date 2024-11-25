using System.Threading.Channels;
using UptimeBot.Console.Core;

namespace UptimeBot.Console.Worker;

public class Worker
{
    private readonly CancellationTokenSource _cts;
    private readonly List<Task> _tasks = [];

    public Worker()
    {
        System.Console.WriteLine("[Worker] starting");
        _cts = new CancellationTokenSource();
        var channel = Channel.CreateUnbounded<WorkerMessage>();
        var pub = new Publisher(channel.Writer, _cts.Token);
        var subs = Enumerable
            .Range(1, 3)
            .Select(id => new Subscriber(channel.Reader, id, _cts.Token))
            .ToList();
        _tasks.Add(pub.StartAsync());
        _tasks.AddRange(subs.Select(s => s.StartAsync()));
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
