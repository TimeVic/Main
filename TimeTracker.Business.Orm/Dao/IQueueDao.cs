using Domain.Abstractions;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface IQueueDao: IScopedDomainService, IDisposable
{
    Task<QueueEntity?> GetById(
        long id,
        CancellationToken cancellationToken = default
    );

    Task Push(
        object context,
        QueueChannel channel = QueueChannel.Default,
        CancellationToken cancellationToken = default
    );

    Task<QueueEntity?> GetTop(QueueChannel? channel = null, CancellationToken cancellationToken = default);

    Task MarkAsProcessed(
        QueueEntity item,
        string? error = null,
        CancellationToken cancellationToken = default
    );

    Task<int> CompleteAllPending(CancellationToken cancellationToken = default);
}
