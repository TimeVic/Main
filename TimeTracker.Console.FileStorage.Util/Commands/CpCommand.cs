using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Console.FileStorage.Util.Services.IO;

namespace TimeTracker.Console.FileStorage.Util.Commands;

public class CpCommand: Command
{
    public CpCommand() : base("cp", "'util cp' command allows you to copy data between your local file system and the cloud")
    {
        var sourceArgument = new Argument<string>(
            name: "source",
            description: "Source path"
        );
        this.Add(sourceArgument);
        
        var destinationArgument = new Argument<string>(
            name: "destination",
            description: "Destination path"
        );
        this.Add(destinationArgument);
        
        var recursiveOption = new Option<bool>(
            aliases: ["--recursive", "-r"],
            description: "Recursive search",
            getDefaultValue: () => false
        )
        {
            IsRequired = false
        };
        this.Add(recursiveOption);
        
        var maskOption = new Option<string>(
            aliases: ["--mask", "-m"],
            description: "Search mask. How example: '*' or '*.jpg' or '**/*', etc",
            getDefaultValue: () => "*"
        )
        {
            IsRequired = false
        };
        this.Add(maskOption);

        this.Handler = CommandHandler.Create(CopyHandler);
    }

    public Task CopyHandler(
        string source,
        string destination,
        bool recursive,
        string mask,
        IServiceProvider serviceProvider
    )
    {
        var localFileSearchService = serviceProvider.GetRequiredService<ILocalFileSearchService>();
        
        var filesEnumerator = localFileSearchService.FindEnumerator(source, recursive, mask);
        foreach (var file in filesEnumerator)
        {
            System.Console.WriteLine(file);    
        }
        return Task.CompletedTask;
    }
}
