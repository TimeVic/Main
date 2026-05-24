using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Client.Core.Services;

public interface IUserLocaleService
{
    Task ApplyUserLocaleAsync(UserDto user);
}
