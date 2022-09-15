using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public class PaymentDao: IPaymentDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public PaymentDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<bool> HasAccessAsync(UserEntity user, PaymentEntity? entity)
    {
        if (entity == null)
        {
            return false;
        }

        UserEntity userAlias = null;
        WorkspaceEntity workspaceAlias = null;
        var itemsWithAccessCount = await _sessionProvider.CurrentSession.QueryOver<ClientEntity>()
            .Inner.JoinAlias(item => item.Workspace, () => workspaceAlias)
            .Inner.JoinAlias(item => workspaceAlias.User, () => userAlias)
            .And(item => item.Id == entity.Id)
            .RowCountAsync();
        return itemsWithAccessCount > 0;
    }
    
    public async Task<PaymentEntity?> GetById(long? id)
    {
        if (id == null)
            return null;

        return await _sessionProvider.CurrentSession.Query<PaymentEntity>()
            .Where(item => item.Id == id)
            .FirstOrDefaultAsync();
    }
    
    public async Task<PaymentEntity> CreateAsync(
        ClientEntity client,
        decimal amount,
        DateTime paymentTime,
        long? projectId,
        string? description
    )
    {
        var entity = new PaymentEntity()
        {
            Amount = amount,
            PaymentTime = paymentTime,
            Description = description,
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };
        client.AddPayment(entity);
        var project = client.Projects
            .AsQueryable()
            .FirstOrDefault(item => item.Id == projectId);
        if (project != null)
        {
            project.AddPayment(entity);
        }

        await _sessionProvider.CurrentSession.SaveAsync(entity);
        return entity;
    }
    
    public async Task<PaymentEntity?> UpdatePaymentAsync(
        long paymentId,
        ClientEntity client,
        decimal amount,
        DateTime paymentTime,
        long? projectId,
        string? description    
    )
    {
        var payment = await _sessionProvider.CurrentSession.Query<PaymentEntity>()
            .FirstOrDefaultAsync(item => item.Id == paymentId);
        if (payment != null)
        {
            payment.UpdateTime = DateTime.UtcNow;
            payment.Client = client;
            payment.Amount = amount;
            payment.PaymentTime = paymentTime;
            payment.Description = description;
            var project = payment.Client.Projects
                .AsQueryable()
                .FirstOrDefault(item => item.Id == projectId);
            if (project != null)
            {
                project.AddPayment(payment);
            }
            else
            {
                payment.Project = null;
            }
        }

        await _sessionProvider.CurrentSession.SaveAsync(payment);
        return payment;
    }
    
    public async Task<ListDto<PaymentEntity>> GetListAsync(WorkspaceEntity workspace, int page)
    {
        var offset = PaginationUtils.CalculateOffset(page);
        
        UserEntity userAlias = null;
        ClientEntity clientAlias = null;
        WorkspaceEntity workspaceAlias = null;
        var query = _sessionProvider.CurrentSession.QueryOver<PaymentEntity>()
            .Inner.JoinAlias(item => item.Client, () => clientAlias)
            .Inner.JoinAlias(item => clientAlias.Workspace, () => workspaceAlias)
            .Where(item => workspaceAlias.Id == workspace.Id);
        
        var items = await query
            .OrderBy(item => item.PaymentTime).Desc
            .Skip(offset)
            .Take(PaginationUtils.DefaultPageSize)
            .ListAsync<PaymentEntity>();
        return new ListDto<PaymentEntity>(
            items,
            await query.RowCountAsync()
        );
    }
}
