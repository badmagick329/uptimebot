using System.Threading.Channels;
using UptimeBot.Console.Core;

namespace UptimeBot.Console.Worker;

public class Publisher
{
    private readonly ChannelWriter<WorkerMessage> _writer;
    private readonly CancellationToken _cancellationToken;
    private int _messageId;

    public Publisher(ChannelWriter<WorkerMessage> writer, CancellationToken token)
    {
        _writer = writer;
        _cancellationToken = token;
    }

    public async Task StartAsync()
    {
        try
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                var message = new WorkerMessage(
                    ++_messageId,
                    $"Message {_messageId}",
                    DateTime.UtcNow
                );

                await _writer.WriteAsync(message, _cancellationToken);
                System.Console.WriteLine($"[Publisher] Sent: {message}");
                await Task.Delay(1000, _cancellationToken); // Send every second
            }
        }
        catch (OperationCanceledException)
        {
            System.Console.WriteLine("[Publisher] Stopping...");
        }
    }
}
