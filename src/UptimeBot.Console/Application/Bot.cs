namespace UptimeBot.Console.Application;

using System.Diagnostics;
using System.Threading.Channels;
using CommunityToolkit.HighPerformance;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using UptimeBot.Console.Commands;
using UptimeBot.Console.Core.Config;
using UptimeBot.Console.Core.Worker;
using UptimeBot.Console.Infrastructure.Repository;

public class Bot
{
    private readonly DiscordClient _client;
    private readonly BotConfig _config;
    private readonly Func<Task> _shutdownHandler;
    private readonly IWorkerMessageRepository _workerMessageRepository;
    private readonly Channel<NotificationMessage> _notifications;

    public Bot(
        BotConfig config,
        IWorkerMessageRepository workerMessageRepository,
        Func<Task> shutdownHandler,
        Channel<NotificationMessage> notifications
    )
    {
        _shutdownHandler = shutdownHandler;
        _config = config;
        _workerMessageRepository = workerMessageRepository;
        _notifications = notifications;

        var builder = DiscordClientBuilder.CreateDefault(
            _config.Token,
            DiscordIntents.MessageContents | DiscordIntents.AllUnprivileged
        );
        AddCommands(builder);
        AddMessageCreatedHandler(builder);

        var status = new DiscordActivity("with fire", DiscordActivityType.Playing);
        _client = builder.Build();
        Debug.Assert(_client != null);
    }

    private void AddCommands(DiscordClientBuilder builder)
    {
        builder.UseCommands(
            (IServiceProvider serviceProvider, CommandsExtension extension) =>
            {
                extension.AddCommands([typeof(PingCommand)]);
                TextCommandProcessor textCommandProcessor =
                    new(
                        new()
                        {
                            PrefixResolver = new DefaultPrefixResolver(
                                true,
                                [_config.Prefix]
                            ).ResolvePrefixAsync,
                        }
                    );
                extension.AddProcessor(textCommandProcessor);
            },
            new CommandsConfiguration()
            {
                RegisterDefaultCommandProcessors = true,
                DebugGuildId = _config.DevGuildId,
            }
        );
    }

    private void AddMessageCreatedHandler(DiscordClientBuilder builder)
    {
        builder.ConfigureEventHandlers(b => b.HandleMessageCreated(MessageCreatedHandler));
    }

    private async Task MessageCreatedHandler(DiscordClient s, MessageCreatedEventArgs e)
    {
        System.Console.WriteLine("Message received: " + e.Message.Content);

        if (
            e.Author.Id == _config.Owner
            && e.Message.Content.StartsWith("ping", StringComparison.OrdinalIgnoreCase)
        )
        {
            try
            {
                await e.Message.RespondAsync("pong!");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Error: " + ex.Message);
            }
            var user = await s.GetUserAsync(_config.Owner);
            var dmChannel = await user.CreateDmChannelAsync();
            await dmChannel.SendMessageAsync("pong!");
        }
    }

    public async Task RunAndWaitAsync()
    {
        using var cts = new CancellationTokenSource();
        await _client.ConnectAsync();

        AppDomain.CurrentDomain.ProcessExit += async (sender, eventArgs) =>
        {
            System.Console.WriteLine("SIGTERM received, exiting...");
            cts.Cancel();
            System.Console.WriteLine("Waiting for shutdown handler...");
            await _shutdownHandler();
        };
        System.Console.CancelKeyPress += async (sender, e) =>
        {
            System.Console.WriteLine("Ctrl+C pressed, exiting...");
            e.Cancel = true;
            cts.Cancel();
            System.Console.WriteLine("Waiting for shutdown handler...");
            await _shutdownHandler();
        };

        _ = StartNotificationListenerAsync(_notifications.Reader, cts.Token);
        await WaitForCancellationAsync(cts.Token);
    }

    private static Task WaitForCancellationAsync(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource();
        var registration = cancellationToken.Register(() =>
        {
            tcs.TrySetResult();
        });

        tcs.Task.ContinueWith(_ => registration.Dispose(), TaskScheduler.Default);

        return tcs.Task;
    }

    private async Task StartNotificationListenerAsync(
        ChannelReader<NotificationMessage> reader,
        CancellationToken cancellationToken
    )
    {
        try
        {
            System.Console.WriteLine("Notification listener starting...");
            while (!cancellationToken.IsCancellationRequested)
            {
                var notification = await reader.ReadAsync(cancellationToken);
                await Task.Delay(1500, cancellationToken);
                while (reader.TryRead(out NotificationMessage? newMessage))
                {
                    if (newMessage is null)
                    {
                        break;
                    }
                    notification.Content += $"\n{newMessage.Content}";
                    if (notification.Content.Count('\n') > 20)
                    {
                        break;
                    }
                    await Task.Delay(1500, cancellationToken);
                }
                await SendDm(notification.Content);
            }
        }
        catch (OperationCanceledException)
        {
            System.Console.WriteLine("Notification listener stopping...");
        }
    }

    private async Task<bool> SendDm(string content)
    {
        try
        {
            var user = await _client.GetUserAsync(_config.Owner);
            var dmChannel = await user.CreateDmChannelAsync();
            await dmChannel.SendMessageAsync(content);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
