namespace UptimeBot.Console.Core.Config;

public class BotConfig
{
    public required string Token { get; set; }
    public required string Prefix { get; set; }
    public required ulong Owner { get; set; }
    public required ulong DevGuildId { get; set; }
    public List<string> UrlsToPing { get; set; } = [];
}
