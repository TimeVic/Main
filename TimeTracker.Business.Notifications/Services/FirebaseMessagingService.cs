using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using TimeTracker.Business.Common.Utils;

namespace TimeTracker.Business.Notifications.Services;

public class FirebaseMessagingService: IFirebaseMessagingService
{
    private const string CredentialsFilepath = "../../../../.credentials/firebase-credentials.json";
    
    private readonly GoogleCredential _credentials;
    
    public FirebaseMessagingService()
    {
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

    public async Task SendMessage(string toToken, string title, string body)
    {
        var message = new Message()
        {
            // Data = data,
            Token = toToken,
            Notification = new FirebaseAdmin.Messaging.Notification()
            {
                Body = body,
                Title = title,
                ImageUrl = "https://timevic.com/img/logo/black/clock-128.png"
            }
        };

        await SendMessage(message);
    }

    public async Task SendMessage(string toToken, Dictionary<string, string> data)
    {
        var message = new Message()
        {
            Token = toToken,
            Data = data
        };

        await SendMessage(message);
    }

    private async Task SendMessage(Message message)
    {
        // Send a message to the device corresponding to the provided
        // registration token.
        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
        catch (Exception e)
        {
                
        }
    }
}
