using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Console.FileStorage.Util.Commands;
using TimeTracker.Console.FileStorage.Util.Core;
using TimeTracker.Console.FileStorage.Util.Services.Auth;
using TimeTracker.Console.FileStorage.Util.Services.Configuration;
using TimeTracker.Console.FileStorage.Util.Services.Http;
using TimeTracker.Console.FileStorage.Util.Services.IO;

namespace TimeTracker.Console.FileStorage.Util;

internal class Program
{
    public async static Task Main(string[] args)
    {
        var rootCommand = new RootCommand(@"
            fsutil is a application that lets you access TimeVic storage from the command line.
        ".Trim())
        {
            new CpCommand(),
            new InitCommand()
        };
        var builder = new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .UseDependencyInjection(services =>
            {
                services.AddSingleton<IConfigurationService, ConfigurationService>();
                services.AddSingleton<IHttpService, HttpService>();
                services.AddSingleton<ISecurityService, SecurityService>();
                services.AddSingleton<ILocalFileSearchService, LocalFileSearchService>();
            });

        await builder.Build().InvokeAsync("init --access-token=123 --secret-token=456");
    }
}
