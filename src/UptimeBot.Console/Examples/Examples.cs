using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using UptimeBot.Console.Commands;
using UptimeBot.Console.Config;

namespace UptimeBot.Console.Examples;

public static class Examples
{
    public static async Task CommandsExample(
        Func<Task> shutdownHandler,
        CancellationTokenSource cts
    )
    {
        var config = BotConfigLoader.CreateConfig();

        var builder = DiscordClientBuilder.CreateDefault(
            config.Token,
            DiscordIntents.MessageContents | DiscordIntents.AllUnprivileged
        );

        builder.UseCommands(
            (IServiceProvider serviceProvider, CommandsExtension extension) =>
            {
                extension.AddCommands([typeof(PingCommand)]);
                TextCommandProcessor textCommandProcessor =
                    new(
                        new()
                        {
                            // The default behavior is that the bot reacts to direct
                            // mentions and to the "!" prefix. If you want to change
                            // it, you first set if the bot should react to mentions
                            // and then you can provide as many prefixes as you want.
                            PrefixResolver = new DefaultPrefixResolver(
                                true,
                                [config.Prefix]
                            ).ResolvePrefixAsync,
                        }
                    );
                extension.AddProcessor(textCommandProcessor);
            },
            new CommandsConfiguration()
            {
                RegisterDefaultCommandProcessors = true,
                DebugGuildId = config.DevGuildId,
            }
        );
        DiscordActivity status = new("with fire", DiscordActivityType.Playing);
        var client = builder.Build();
        Debug.Assert(client != null);
        await client.ConnectAsync();
        await WaitForCtrlC(shutdownHandler, cts);
    }

    public static async Task BasicExample()
    {
        var config = BotConfigLoader.CreateConfig();
        var builder = DiscordClientBuilder.CreateDefault(
            config.Token,
            DiscordIntents.MessageContents | DiscordIntents.AllUnprivileged
        );

        builder.ConfigureEventHandlers(b =>
            b.HandleMessageCreated(
                async (s, e) =>
                {
                    System.Console.WriteLine("Message received: " + e.Message.Content);

                    if (e.Message.Content.StartsWith("ping", StringComparison.OrdinalIgnoreCase))
                    {
                        await e.Message.RespondAsync("pong!");
                    }
                }
            )
        );

        var client = builder.Build();
        Debug.Assert(client != null);
        await client.ConnectAsync();
        await Task.Delay(-1);
    }

    static async Task WaitForCtrlC(Func<Task> shutdownHandler, CancellationTokenSource cts)
    {
        System.Console.WriteLine("Press Ctrl+C to exit...");

        System.Console.CancelKeyPress += async (sender, e) =>
        {
            System.Console.WriteLine("Ctrl+C pressed, exiting...");
            e.Cancel = true;
            await Task.Delay(1000);
            System.Console.WriteLine("Sending cancellation signal...");
            cts.Cancel();
            System.Console.WriteLine("Waiting for shutdown handler...");
            await shutdownHandler();
        };
        try
        {
            System.Console.WriteLine("Waiting for cancellation");
            await WaitForCancellationAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            System.Console.WriteLine("Cancellation requested");
        }
        System.Console.WriteLine("Exiting...");
    }

    static Task WaitForCancellationAsync(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource();
        var registration = cancellationToken.Register(() =>
        {
            tcs.TrySetResult();
        });

        tcs.Task.ContinueWith(_ => registration.Dispose(), TaskScheduler.Default);

        return tcs.Task;
    }
}
