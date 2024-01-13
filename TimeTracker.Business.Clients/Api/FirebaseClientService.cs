using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using TimeTracker.Business.Common.Utils;

namespace TimeTracker.Business.Clients.Api;

public class FirebaseClientService: IFirebaseClientService
{
    private const string CredentialsFilepath = "../../../../.credentials/firebase-credentials.json";
 
    private readonly ILogger<FirebaseClientService> _logger;
    
    private readonly GoogleCredential _credentials;
    
    public FirebaseClientService(ILogger<FirebaseClientService> logger)
    {
        _logger = logger;
        var filePath = Path.Combine(AssemblyUtils.GetAssemblyPath(), CredentialsFilepath);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Google Cloud credentials file not found: {filePath}");
        }

        using var credentialsStream = new FileStream(
            Path.Combine(AssemblyUtils.GetAssemblyPath(), CredentialsFilepath),
            FileMode.Open,
            FileAccess.Read
        );
        _credentials = GoogleCredential.FromStream(credentialsStream);
        FirebaseApp.Create(new AppOptions()
        {
            Credential = _credentials
        });
    }

    public Task<bool> SendMessage(string toToken, string title, string body)
    {
        var message = new Message()
        {
            Token = toToken,
            Webpush = new WebpushConfig()
            {
                Notification = new WebpushNotification()
                {
                    Body = body,
                    Title = title,
                    Icon = "https://timevic.com/img/logo/black/clock-128.png"
                }
            }
        };

        return SendMessage(message);
    }

    public Task<bool> SendMessage(string toToken, Dictionary<string, string> data)
    {
        var message = new Message()
        {
            Token = toToken,
            Data = data
        };

        return SendMessage(message);
    }

    public async Task<bool> ValidateToken(string token)
    {
        // Send a message to the device corresponding to the provided
        // registration token.
        try
        {
            var message = new MulticastMessage()
            {
                Tokens = new List<string>() { token }
            };
            var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
            return response.SuccessCount > 0;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
        return false;
    }
    
    private async Task<bool> SendMessage(Message message)
    {
        // Send a message to the device corresponding to the provided
        // registration token.
        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return !string.IsNullOrEmpty(response);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
        return false;
    }
}
