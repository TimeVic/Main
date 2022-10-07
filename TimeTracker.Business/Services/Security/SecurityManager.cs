using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Security;

public class SecurityManager: ISecurityManager
{
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public SecurityManager(
        IWorkspaceAccessService workspaceAccessService
    )
    {
        _workspaceAccessService = workspaceAccessService;
    }

    public async Task<bool> HasAccess<TEntity>(AccessLevel accessLevel, UserEntity user, TEntity? entity)
    {
        if (entity == null)
            return false;
        
        if (entity is TimeEntryEntity entryEntity)
        {
            return await HasAccessToTimeEntry(accessLevel, user, entryEntity);
        }
        if (entity is ProjectEntity projectEntity)
        {
            return await HasAccessToProject(accessLevel, user, projectEntity);
        }
        if (entity is ClientEntity clientEntity)
        {
            return await HasAccessToClientAsync(accessLevel, user, clientEntity);
        }
        if (entity is PaymentEntity paymentEntity)
        {
            return await HasAccessToPayment(accessLevel, user, paymentEntity);
        }

        throw new NotImplementedException($"Security checking not implemented for {entity?.GetTypeName()}");
    }

    private async Task<bool> HasAccessToTimeEntry(AccessLevel accessLevel, UserEntity user, TimeEntryEntity timeEntry)
    {
        var accessType = await _workspaceAccessService.GetAccessTypeAsync(user, timeEntry.Workspace);
        return accessType == MembershipAccessType.Owner 
            || accessType == MembershipAccessType.Manager
            || (
                accessLevel == AccessLevel.Write 
                && accessType == MembershipAccessType.User
                && timeEntry.User.Id == user.Id
            )
            || (
                accessLevel == AccessLevel.Read 
                && accessType == MembershipAccessType.User
            );
    }
    
    private async Task<bool> HasAccessToClientAsync(AccessLevel accessLevel, UserEntity user, ClientEntity client)
    {
        var accessType = await _workspaceAccessService.GetAccessTypeAsync(user, client.Workspace);
        return accessType == MembershipAccessType.Owner 
            || accessType == MembershipAccessType.Manager
            || (
                accessLevel == AccessLevel.Read 
                && accessType == MembershipAccessType.User
            );
    }
    
    private async Task<bool> HasAccessToProject(AccessLevel accessLevel, UserEntity user, ProjectEntity project)
    {
        var accessType = await _workspaceAccessService.GetAccessTypeAsync(user, project);
        return accessType == MembershipAccessType.Owner 
            || accessType == MembershipAccessType.Manager
            || (
                accessLevel == AccessLevel.Read 
                && accessType == MembershipAccessType.User
            );
    }
    
    private async Task<bool> HasAccessToPayment(AccessLevel accessLevel, UserEntity user, PaymentEntity payment)
    {
        var accessType = await _workspaceAccessService.GetAccessTypeAsync(user, payment.Workspace);
        return accessType != null
            && payment.User.Id == user.Id;
    }
}
