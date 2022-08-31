using Domain.Abstractions;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface IQueueDao: IScopedDomainService, IDisposable
{
    Task Push<T>(
        T context,
        QueueChannel channel = QueueChannel.Default,
        CancellationToken cancellationToken = default
    ) where T: struct;

    Task<QueueEntity?> GetTop(QueueChannel? channel = null, CancellationToken cancellationToken = default);
}
