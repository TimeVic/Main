using Domain.Abstractions;

namespace TimeTracker.Business.Clients.Api;

public interface IFirebaseClientService: IDomainService
{
    Task<bool> SendMessage(string toToken, Dictionary<string, string> data);

    Task<bool> SendMessage(string toToken, string title, string body);
    
    Task<bool> ValidateToken(string token);
}
