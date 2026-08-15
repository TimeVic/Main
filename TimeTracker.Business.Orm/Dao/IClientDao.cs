using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao;

public interface IClientDao: IDomainService
{
    Task<ClientEntity> CreateAsync(WorkspaceEntity workspace, string name);

    Task<ListDto<ClientEntity>> GetListAsync(
        WorkspaceEntity workspace,
        int page,
        UserEntity? user = null,
        MembershipAccessType? accessType = null
    );

    Task<ClientEntity?> GetById(Guid? clientId, WorkspaceEntity? workspace = null);
}
