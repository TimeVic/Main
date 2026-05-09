using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao;

public interface IMemberPaymentDao: IDomainService
{
    Task<MemberPaymentEntity?> GetById(Guid? id);
    
    Task<MemberPaymentEntity> CreateAsync(
        WorkspaceMemberEntity member,
        ProjectEntity project,
        decimal amount,
        DateTime paymentTime,
        string? description = null
    );

    Task<MemberPaymentEntity> CreateAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        ProjectEntity project,
        decimal amount,
        DateTime paymentTime,
        string? description = null
    );

    Task<MemberPaymentEntity?> UpdateMemberPaymentAsync(
        Guid paymentId,
        WorkspaceMemberEntity member,
        ProjectEntity project,
        decimal amount,
        DateTime paymentTime,
        string? description
    );
    
    Task<ListDto<MemberPaymentEntity>> GetListAsync(WorkspaceMemberEntity member, int page);

    Task<ListDto<MemberPaymentEntity>> GetListAsync(WorkspaceEntity workspace, int page);
    
    Task<ListDto<MemberPaymentEntity>> GetListAsync(WorkspaceEntity workspace, UserEntity user, int page);
}
