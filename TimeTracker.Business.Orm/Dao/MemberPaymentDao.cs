using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao;

public class MemberPaymentDao: IMemberPaymentDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public MemberPaymentDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<MemberPaymentEntity?> GetById(Guid? id)
    {
        if (id == null)
            return null;

        return await _sessionProvider.CurrentSession.Query<MemberPaymentEntity>()
            .Where(item => item.Id == id)
            .FirstOrDefaultAsync();
    }
    
    public async Task<MemberPaymentEntity> CreateAsync(
        WorkspaceMemberEntity member,
        ProjectEntity project,
        decimal amount,
        DateTime paymentTime,
        string? description = null
    )
    {
        var workspace = member.Workspace;

        if (project.Client.Workspace.Id != workspace.Id)
        {
            throw new DataInconsistencyException($"This workspace does not contain project: {project.Id}");
        }

        var entity = new MemberPaymentEntity
        {
            Member = member,
            Amount = amount,
            PaymentTime = paymentTime,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Project = project
        };
        project.AddMemberPayment(entity);

        await _sessionProvider.CurrentSession.SaveAsync(entity);
        return entity;
    }

    public async Task<MemberPaymentEntity> CreateAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        ProjectEntity project,
        decimal amount,
        DateTime paymentTime,
        string? description = null
    )
    {
        var member = await GetMemberAsync(workspace, user);
        return await CreateAsync(member, project, amount, paymentTime, description);
    }
    
    public async Task<MemberPaymentEntity?> UpdateMemberPaymentAsync(
        Guid paymentId,
        WorkspaceMemberEntity member,
        ProjectEntity project,
        decimal amount,
        DateTime paymentTime,
        string? description    
    )
    {
        var payment = await _sessionProvider.CurrentSession.Query<MemberPaymentEntity>()
            .FirstOrDefaultAsync(item => item.Id == paymentId);
        if (payment == null)
        {
            return null;
        }

        payment.UpdatedAt = DateTime.UtcNow;
        payment.Member = member;
        payment.Amount = amount;
        payment.PaymentTime = paymentTime;
        payment.Description = description;
        if (project.Client.Workspace.Id != member.Workspace.Id)
        {
            throw new DataInconsistencyException($"This workspace does not contain project: {project.Id}");
        }

        project.AddMemberPayment(payment);

        await _sessionProvider.CurrentSession.SaveAsync(payment);
        return payment;
    }
    
    public async Task<ListDto<MemberPaymentEntity>> GetListAsync(WorkspaceMemberEntity member, int page)
    {
        var offset = PaginationUtils.CalculateOffset(page);
        var query = _sessionProvider.CurrentSession.Query<MemberPaymentEntity>()
            .Where(item => item.Member.Id == member.Id);
        
        var items = await BuildListQuery(query)
            .OrderByDescending(item => item.PaymentTime)
            .Skip(offset)
            .Take(PaginationUtils.DefaultPageSize)
            .ToListAsync();
        return new ListDto<MemberPaymentEntity>(
            items,
            await query.CountAsync()
        );
    }

    public async Task<ListDto<MemberPaymentEntity>> GetListAsync(WorkspaceEntity workspace, int page)
    {
        var offset = PaginationUtils.CalculateOffset(page);
        var query = _sessionProvider.CurrentSession.Query<MemberPaymentEntity>()
            .Where(item => item.Member.Workspace.Id == workspace.Id);

        var items = await BuildListQuery(query)
            .OrderByDescending(item => item.PaymentTime)
            .Skip(offset)
            .Take(PaginationUtils.DefaultPageSize)
            .ToListAsync();
        return new ListDto<MemberPaymentEntity>(
            items,
            await query.CountAsync()
        );
    }

    public async Task<ListDto<MemberPaymentEntity>> GetListAsync(WorkspaceEntity workspace, UserEntity user, int page)
    {
        var member = await GetMemberAsync(workspace, user);
        return await GetListAsync(member, page);
    }

    private async Task<WorkspaceMemberEntity> GetMemberAsync(WorkspaceEntity workspace, UserEntity user)
    {
        var member = await _sessionProvider.CurrentSession.Query<WorkspaceMemberEntity>()
            .Where(item => item.Workspace.Id == workspace.Id && item.User.Id == user.Id)
            .FirstOrDefaultAsync();
        if (member == null)
        {
            throw new DataInconsistencyException($"This workspace does not contain member for user: {user.Id}");
        }

        return member;
    }

    private static IQueryable<MemberPaymentEntity> BuildListQuery(IQueryable<MemberPaymentEntity> query)
    {
        return query
            .Fetch(item => item.Project)
            .ThenFetch(item => item.Client)
            .Fetch(item => item.Member)
            .ThenFetch(item => item.User);
    }
}
