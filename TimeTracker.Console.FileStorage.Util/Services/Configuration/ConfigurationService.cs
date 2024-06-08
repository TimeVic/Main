using Newtonsoft.Json;
using TimeTracker.Console.FileStorage.Util.Constants;

namespace TimeTracker.Console.FileStorage.Util.Services.Configuration;

public class ConfigurationService: IConfigurationService
{
    private string _configurationFile = StorageUtilConstants.ConfigFile;
    
    private ConfigurationDto _configuration = new();
    
    public ConfigurationService()
    {
        if (!File.Exists(_configurationFile))
        {
            Directory.CreateDirectory(StorageUtilConstants.UtilDir);
            File.WriteAllText(_configurationFile, "{}");
        }
        LoadConfig();
    }

    public string? GetAccessKey()
    {
        return _configuration.AccessKey;
    }
    
    public string? GetSecretKey()
    {
        return _configuration.SecretKey;
    }
    
    public void SetCredentials(string accessKey, string secretKey)
    {
        _configuration.AccessKey = accessKey;
        _configuration.SecretKey = secretKey;
        SaveConfig();
    }

    private void SaveConfig()
    {
        var serializedConfig = JsonConvert.SerializeObject(_configuration);
        File.WriteAllText(StorageUtilConstants.ConfigFile, serializedConfig);
    }
    
    private void LoadConfig()
    {
        var configFileContent = File.ReadAllText(StorageUtilConstants.ConfigFile);
        _configuration = JsonConvert.DeserializeObject<ConfigurationDto>(configFileContent)!;
    }
}
