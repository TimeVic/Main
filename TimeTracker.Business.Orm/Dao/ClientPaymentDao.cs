using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao;

public class ClientPaymentDao: IClientPaymentDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public ClientPaymentDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<ClientPaymentEntity?> GetById(Guid? id)
    {
        if (id == null)
            return null;

        return await _sessionProvider.CurrentSession.Query<ClientPaymentEntity>()
            .Where(item => item.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<ClientPaymentEntity> CreateAsync(
        ClientEntity client,
        decimal amount,
        DateTime paymentTime,
        Guid? projectId = null,
        string? description = null
    )
    {
        var entity = new ClientPaymentEntity
        {
            Client = client,
            Amount = amount,
            PaymentTime = paymentTime,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var project = client.Projects.FirstOrDefault(item => item.Id == projectId);
        if (project != null)
        {
            entity.Project = project;
        }

        await _sessionProvider.CurrentSession.SaveAsync(entity);
        return entity;
    }

    public async Task<ClientPaymentEntity?> UpdateClientPaymentAsync(
        Guid paymentId,
        ClientEntity client,
        decimal amount,
        DateTime paymentTime,
        Guid? projectId,
        string? description
    )
    {
        var payment = await _sessionProvider.CurrentSession.Query<ClientPaymentEntity>()
            .FirstOrDefaultAsync(item => item.Id == paymentId);
        if (payment != null)
        {
            if (client.Workspace.Id != payment.Client.Workspace.Id)
            {
                throw new DataInconsistencyException($"This workspace does not contain client: {client.Id}");
            }

            payment.UpdatedAt = DateTime.UtcNow;
            payment.Client = client;
            payment.Amount = amount;
            payment.PaymentTime = paymentTime;
            payment.Description = description;
            payment.Project = client.Projects.FirstOrDefault(item => item.Id == projectId);
        }

        await _sessionProvider.CurrentSession.SaveAsync(payment);
        return payment;
    }

    public async Task<ListDto<ClientPaymentEntity>> GetListAsync(WorkspaceEntity workspace, int page)
    {
        var offset = PaginationUtils.CalculateOffset(page);
        var query = _sessionProvider.CurrentSession.Query<ClientPaymentEntity>()
            .Where(item => item.Client.Workspace.Id == workspace.Id);

        var items = await query
            .Fetch(item => item.Client)
            .Fetch(item => item.Project)
            .OrderByDescending(item => item.PaymentTime)
            .Skip(offset)
            .Take(PaginationUtils.DefaultPageSize)
            .ToListAsync();

        return new ListDto<ClientPaymentEntity>(
            items,
            await query.CountAsync()
        );
    }
}
