using System.Threading.Channels;
using UptimeBot.Console.Examples;
using UptimeBot.Console.Worker;

class Program
{
    public static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        var worker = new Worker();

        try
        {
            await Examples.CommandsExample(worker.Shutdown, cts);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception: {e.Message}");
        }
    }
}
