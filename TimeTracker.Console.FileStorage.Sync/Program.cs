using System.CommandLine;

namespace TimeTracker.Console.FileStorage.Sync;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var rootCommand = new RootCommand("Sample command-line app");

        var cpCommand = new Command("cp", "command allows you to copy data between your local file system and the cloud");
        cpCommand.SetHandler(() =>
        {
            System.Console.WriteLine("Sub command 1");
        });
        rootCommand.Add(cpCommand);
        
        rootCommand.SetHandler(() =>
        {
            System.Console.WriteLine("Hello world!");
        });

        await rootCommand.InvokeAsync(args);
    }
}
