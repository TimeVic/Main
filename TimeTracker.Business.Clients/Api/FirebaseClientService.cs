using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TimeTracker.Business.Common.Utils;

namespace TimeTracker.Business.Clients.Api;

public class FirebaseClientService: IFirebaseClientService
{
    private const string CredentialsFilepath = "../../../../.credentials/firebase-credentials.json";
    private static readonly object FirebaseAppInitializationLock = new();
    private static FirebaseApp? _firebaseApp;
 
    private readonly ILogger<FirebaseClientService> _logger;

    public FirebaseClientService(
        ILogger<FirebaseClientService> logger,
        IConfiguration configuration
    )
    {
        _logger = logger;

        var credentials = BuildCredentials(configuration);
        EnsureDefaultFirebaseAppInitialized(credentials);
    }

    private static GoogleCredential BuildCredentials(IConfiguration configuration)
    {
        var filePath = Path.Combine(AssemblyUtils.GetAssemblyPath(), CredentialsFilepath);
        if (File.Exists(filePath))
        {
            using var credentialsStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read
            );
            return CredentialFactory.FromStream<ServiceAccountCredential>(credentialsStream).ToGoogleCredential();
        }

        var jsonConfiguration = configuration.GetValue<string>("Google:Firebase:Credentials")
                                ?? throw new FileNotFoundException("Firebase credentials configuration not found");
        return CredentialFactory.FromJson<ServiceAccountCredential>(jsonConfiguration).ToGoogleCredential();
    }

    private static void EnsureDefaultFirebaseAppInitialized(GoogleCredential credentials)
    {
        lock (FirebaseAppInitializationLock)
        {
            if (_firebaseApp != null)
            {
                return;
            }

            try
            {
                _firebaseApp = FirebaseApp.DefaultInstance;
            }
            catch (InvalidOperationException)
            {
                _firebaseApp = FirebaseApp.Create(new AppOptions
                {
                    Credential = credentials
                });
            }
        }
    }

    public Task<bool> SendMessage(string toToken, string title, string body, string? link = null)
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

        if (!string.IsNullOrWhiteSpace(link))
        {
            message.Data = new Dictionary<string, string>
            {
                ["url"] = link
            };
            message.Webpush.FcmOptions = new WebpushFcmOptions
            {
                Link = link
            };
        }

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
            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
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
