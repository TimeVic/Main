namespace TimeTracker.Console.FileStorage.Util.Services.Configuration;

public interface IConfigurationService
{
    string? GetAccessKey();

    string? GetSecretKey();

    void SetCredentials(string accessKey, string secretKey);
}
