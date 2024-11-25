namespace UptimeBot.Console.Config;

public class BotConfig
{
    public required string Token { get; set; }
    public required string Prefix { get; set; }
    public required string Owner { get; set; }
    public required ulong DevGuildId { get; set; }
}
