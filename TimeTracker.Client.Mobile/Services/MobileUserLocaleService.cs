using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Services;

namespace TimeTracker.Client.Mobile.Services;

public class MobileUserLocaleService : IUserLocaleService
{
    public Task ApplyUserLocaleAsync(UserDto user)
    {
        return Task.CompletedTask;
    }
}
