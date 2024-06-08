using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using TimeTracker.Business.Extensions;
using TimeTracker.Console.FileStorage.Util.Services.Configuration;
using TimeTracker.Console.FileStorage.Util.Services.IO;

namespace TimeTracker.Console.FileStorage.Util.Commands;

public class InitCommand: Command
{
    public InitCommand() : base("init", "'util init' command allows you to initialize app")
    {
        var accessTokenOption = new Option<string>("--access-token", "Access token")
        {
            IsRequired = true
        };
        var secretTokenOption = new Option<string>("--secret-token", "Secret token")
        {
            IsRequired = true
        };
        
        this.Add(accessTokenOption);
        this.Add(secretTokenOption);
        
        this.Handler = CommandHandler.Create(InitHandler);
    }

    public Task InitHandler(
        string accessToken,
        string secretToken,
        IConfigurationService configurationService
    )
    {
        configurationService.SetCredentials(accessToken, secretToken);
        return Task.CompletedTask;
    }
}
