using Domain.Abstractions;

namespace TimeTracker.Business.Notifications.Services;

public interface IFirebaseMessagingService: IDomainService
{
    Task SendMessage(string toToken, Dictionary<string, string> data);
}
