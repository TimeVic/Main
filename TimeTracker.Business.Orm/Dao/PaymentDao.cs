using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public class PaymentDao: IPaymentDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public PaymentDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<PaymentEntity> Create(
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
}
