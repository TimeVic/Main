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
        ClientEntity client,
        decimal amount,
        DateTime paymentTime,
        Guid? projectId = null,
        string? description = null
    )
    {
        var workspace = member.Workspace;

        if (workspace.Clients.All(item => item.Id != client.Id))
        {
            throw new DataInconsistencyException($"This workspace does not contain client: {client.Id}");
        }

        var entity = new MemberPaymentEntity
        {
            Member = member,
            Amount = amount,
            PaymentTime = paymentTime,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Client = client
        };
        client.AddMemberPayment(entity);
        var project = client.Projects.FirstOrDefault(item => item.Id == projectId);
        if (project != null)
        {
            project.AddMemberPayment(entity);
        }

        await _sessionProvider.CurrentSession.SaveAsync(entity);
        return entity;
    }

    public async Task<MemberPaymentEntity> CreateAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        ClientEntity client,
        decimal amount,
        DateTime paymentTime,
        Guid? projectId = null,
        string? description = null
    )
    {
        var member = await GetMemberAsync(workspace, user);
        return await CreateAsync(member, client, amount, paymentTime, projectId, description);
    }
    
    public async Task<MemberPaymentEntity?> UpdateMemberPaymentAsync(
        Guid paymentId,
        ClientEntity client,
        decimal amount,
        DateTime paymentTime,
        Guid? projectId,
        string? description    
    )
    {
        var payment = await _sessionProvider.CurrentSession.Query<MemberPaymentEntity>()
            .FirstOrDefaultAsync(item => item.Id == paymentId);
        if (payment != null)
        {
            payment.UpdatedAt = DateTime.UtcNow;
            payment.Client = client;
            payment.Amount = amount;
            payment.PaymentTime = paymentTime;
            payment.Description = description;
            var project = payment.Client.Projects.FirstOrDefault(item => item.Id == projectId);
            if (project != null)
            {
                project.AddMemberPayment(payment);
            }
            else
            {
                payment.Project = null!;
            }
        }

        await _sessionProvider.CurrentSession.SaveAsync(payment);
        return payment;
    }
    
    public async Task<ListDto<MemberPaymentEntity>> GetListAsync(WorkspaceMemberEntity member, int page)
    {
        var offset = PaginationUtils.CalculateOffset(page);
        var query = _sessionProvider.CurrentSession.QueryOver<MemberPaymentEntity>()
            .Where(item => item.Member.Id == member.Id);
        
        var items = await query
            .OrderBy(item => item.PaymentTime).Desc
            .Skip(offset)
            .Take(PaginationUtils.DefaultPageSize)
            .ListAsync<MemberPaymentEntity>();
        return new ListDto<MemberPaymentEntity>(
            items,
            await query.RowCountAsync()
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
}
