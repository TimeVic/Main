using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp.Model;

namespace TimeTracker.Business.Services.ExternalClients.ClickUp;

public interface IClickUpClient: IDomainService
{
    Task<SetTimeEntryResponseDto?> SendTimeEntryAsync(TimeEntryEntity timeEntry);
}
