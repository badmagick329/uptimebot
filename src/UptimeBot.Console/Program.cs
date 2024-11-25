using System.Threading.Channels;
using UptimeBot.Console.Examples;

class Program
{
    public static async Task Main(string[] args)
    {
        var channel = Channel.CreateUnbounded<string>();

        try
        {
            await Examples.CommandsExample();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception: {e.Message}");
        }
    }
}
