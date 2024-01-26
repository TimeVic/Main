using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using TimeTracker.Business.Common.Utils;

namespace TimeTracker.Business.Clients.Api;

public class FirebaseClientServiceMock: IFirebaseClientService
{
    public const string SuccessToken = "SuccessToken";
    public const string SuccessToken2 = "SuccessToken2";

    public ICollection<Message> SentMessages = new List<Message>();
    
    public FirebaseClientServiceMock()
    {
        
    }

    public void Reset()
    {
        SentMessages.Clear();
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
                    Title = title
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

    public Task<bool> ValidateToken(string token)
    {
        return Task.FromResult(token == SuccessToken);
    }
    
    private async Task<bool> SendMessage(Message message)
    {
        if (message.Token is SuccessToken or SuccessToken2)
        {
            SentMessages.Add(message);
            return true;
        }
        return false;
    }
}
