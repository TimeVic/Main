using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Entity;

public interface IProjectService: IDomainService
{
    Task<decimal?> GetUsersHourlyRateForProject(UserEntity user, ProjectEntity? project);
}
