using Domain.Abstractions;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Security;

public interface ISecurityManager: IDomainService
{
    Task<bool> HasAccess<TEntity>(AccessLevel accessLevel, UserEntity user, TEntity entity);
}
