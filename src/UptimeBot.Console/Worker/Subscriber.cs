using System.Threading.Channels;
using UptimeBot.Console.Core;

namespace UptimeBot.Console.Worker;

public class Subscriber
{
    private readonly ChannelReader<WorkerMessage> _reader;
    private readonly CancellationToken _cancellationToken;
    private readonly int _id;

    public Subscriber(ChannelReader<WorkerMessage> reader, int id, CancellationToken token)
    {
        _reader = reader;
        _cancellationToken = token;
        _id = id;
    }

    public async Task StartAsync()
    {
        try
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                var message = await _reader.ReadAsync(_cancellationToken);
                await ProcessMessageAsync(message);
            }
        }
        catch (OperationCanceledException)
        {
            System.Console.WriteLine($"[Subscriber {_id}] Stopping...");
        }
    }

    private async Task ProcessMessageAsync(WorkerMessage message)
    {
        // Simulate processing time
        await Task.Delay(Random.Shared.Next(100, 500), _cancellationToken);
        System.Console.WriteLine($"[Subscriber {_id}] Processed: {message}");
    }
}
