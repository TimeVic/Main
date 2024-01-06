using Domain.Abstractions;

namespace TimeTracker.Business.Clients.Api;

public interface IFirebaseMessagingService: IDomainService
{
    Task SendMessage(string toToken, Dictionary<string, string> data);

    Task SendMessage(string toToken, string title, string body);
    
    Task<bool> ValidateToken(string token);
}
