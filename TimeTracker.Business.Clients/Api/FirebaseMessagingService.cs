using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using TimeTracker.Business.Common.Utils;

namespace TimeTracker.Business.Clients.Api;

public class FirebaseMessagingService: IFirebaseMessagingService
{
    private const string CredentialsFilepath = "../../../../.credentials/firebase-credentials.json";
 
    private readonly ILogger<FirebaseMessagingService> _logger;
    
    private readonly GoogleCredential _credentials;
    
    public FirebaseMessagingService(ILogger<FirebaseMessagingService> logger)
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

    public Task SendMessage(string toToken, string title, string body)
    {
        var message = new Message()
        {
            Token = toToken,
            Notification = new FirebaseAdmin.Messaging.Notification()
            {
                Body = body,
                Title = title,
                ImageUrl = "https://timevic.com/img/logo/black/clock-128.png"
            }
        };

        return SendMessage(message);
    }

    public Task SendMessage(string toToken, Dictionary<string, string> data)
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
    
    private async Task SendMessage(Message message)
    {
        // Send a message to the device corresponding to the provided
        // registration token.
        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            // TODO: Remove Token if failed
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }
}
