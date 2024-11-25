using DSharpPlus.Commands;
using Humanizer;

namespace UptimeBot.Console.Commands;

public class PingCommand
{
    [Command("ping")]
    public static async ValueTask ExecuteAsync(CommandContext context)
    {
        await context.RespondAsync(
            $"Pong! Latency is {context.Client.GetConnectionLatency(context.Guild?.Id ?? 0).Humanize()}"
        );
    }
}
