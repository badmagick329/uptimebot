using System.Threading.Channels;
using UptimeBot.Console.Infrastructure.Repository;

namespace UptimeBot.Console.Core.Worker;

public class Publisher
{
    private readonly ChannelWriter<WorkerMessage> _writer;
    private readonly CancellationToken _cancellationToken;
    private readonly IWorkerMessageRepository _workerMessageRepository;

    public Publisher(
        ChannelWriter<WorkerMessage> writer,
        IWorkerMessageRepository workerMessageRepository,
        CancellationToken token
    )
    {
        _writer = writer;
        _cancellationToken = token;
        _workerMessageRepository = workerMessageRepository;
    }

    public async Task StartAsync()
    {
        try
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                var workerMessages = await _workerMessageRepository.GetAsync();
                if (workerMessages.Any())
                {
                    System.Console.WriteLine("[Publisher] Found message to send...");

                    foreach (var message in workerMessages)
                    {
                        await _writer.WriteAsync(message, _cancellationToken);
                        System.Console.WriteLine($"[Publisher] Sent: {message}");
                    }
                }

                await Task.Delay(1000, _cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            System.Console.WriteLine("[Publisher] Stopping...");
        }
    }
}
