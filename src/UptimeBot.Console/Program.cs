using System.Threading.Channels;
using UptimeBot.Console.Application;
using UptimeBot.Console.Core.Config;
using UptimeBot.Console.Core.Worker;
using UptimeBot.Console.Infrastructure.Repository;

class Program
{
    public static async Task Main(string[] args)
    {
        var config = BotConfigLoader.CreateConfig();

        var workerMessages = new WorkerMessagesFromConf(config.UrlsToPing);
        var worker = new Worker(workerMessages);
        var notificationChannel = Channel.CreateUnbounded<NotificationMessage>();
        worker.AddNotificationHandler(
            async (string notification) =>
            {
                Console.WriteLine("[NotificationHandler] Sending notification...");

                await notificationChannel.Writer.WriteAsync(
                    new NotificationMessage(config.Owner, notification, true)
                );
            }
        );

        try
        {
            var bot = new Bot(config, workerMessages, worker.Shutdown, notificationChannel);
            await Task.Delay(2000);
            worker.Start();
            await bot.RunAndWaitAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception: {e.Message}");
        }
    }
}
