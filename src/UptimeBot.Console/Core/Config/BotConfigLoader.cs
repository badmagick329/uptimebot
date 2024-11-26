using Microsoft.Extensions.Configuration;

namespace UptimeBot.Console.Core.Config;

public static class BotConfigLoader
{
    public static BotConfig CreateConfig(string? settingsFile = null)
    {
        DotNetEnv.Env.Load();
        settingsFile ??= "appsettings.json";
        var configurationRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(settingsFile, optional: false, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        return new BotConfig()
        {
            Token =
                configurationRoot["DiscordToken"]
                ?? throw new InvalidOperationException("Token configuration is missing"),
            Prefix =
                configurationRoot["Prefix"]
                ?? throw new InvalidOperationException("Prefix configuration is missing"),
            Owner = ulong.Parse(
                configurationRoot["DiscordIds:Owner"]
                    ?? throw new InvalidOperationException("Owner configuration is missing")
            ),
            DevGuildId = ulong.Parse(
                configurationRoot["DiscordIds:DevGuild"]
                    ?? throw new InvalidOperationException("DevGuildId configuration is missing")
            ),
            UrlsToPing = configurationRoot.GetSection("UrlsToPing").Get<List<string>>() ?? [],
        };
    }
}
