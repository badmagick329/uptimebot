using System.Threading.Channels;

namespace UptimeBot.Console.Core.Worker;

public class Subscriber
{
    private static readonly HttpClient _client = new();
    private readonly ChannelReader<WorkerMessage> _reader;
    private readonly CancellationToken _cancellationToken;
    private readonly int _id;
    private Func<string, Task>? _notificationHandler;

    public Subscriber(ChannelReader<WorkerMessage> reader, int id, CancellationToken token)
    {
        _reader = reader;
        _cancellationToken = token;
        _id = id;
    }

    public void AddNotificationHandler(Func<string, Task> notificationHandler)
    {
        _notificationHandler = notificationHandler;
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
        if (message.Type == WorkerMessageType.Url)
        {
            try
            {
                using HttpResponseMessage response = await _client.GetAsync(message.Content);
                string notification =
                    $"[Subscriber {_id}] **Pinged** {message.Content} - **{response.StatusCode}**";
                if (_notificationHandler is not null && !response.IsSuccessStatusCode)
                {
                    await _notificationHandler(notification);
                }
                return;
            }
            catch (Exception ex)
            {
                string notification =
                    $"[Subscriber {_id}] **Pinged** {message.Content} - **Failed** with {ex.Message}";
                if (_notificationHandler is not null)
                {
                    await _notificationHandler(notification);
                }
                return;
            }
        }
        await Task.Delay(Random.Shared.Next(100, 500), _cancellationToken);
        System.Console.WriteLine($"[Subscriber {_id}] Processed: {message}");
    }
}
