using Domain.Abstractions;

namespace TimeTracker.Business.Services.Entity;

public interface IAntiForgetTimerService : IDomainService
{
    Task CheckActiveTimersAsync(DateTime currentTime, CancellationToken cancellationToken = default);
}
