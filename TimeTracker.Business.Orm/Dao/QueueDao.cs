using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Transform;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public class QueueDao: IQueueDao
{
    private readonly ILogger<IQueueDao> _logger;
    private readonly ISession _session;

    public QueueDao(IDbSessionProvider sessionProvider, ILogger<IQueueDao> logger)
    {
        _logger = logger;
        _session = sessionProvider.CreateSession();
    }

    public async Task Push(
        object context,
        QueueChannel channel = QueueChannel.Default,
        DateTime? processAt = null,
        QueuePriority? priority = null,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = _session.BeginTransaction();

        try
        {
            var contextType = context.GetType();
            var typeString = string.Join(".", contextType.Namespace, contextType.Name);
            var queueItem = new QueueEntity
            {
                Channel = channel,
                Status = QueueStatus.Pending,
                Priority = priority ?? QueuePriority.Normal,
                ContextType = typeString,
                ContextData = JsonHelper.SerializeToString(context),
                ProcessAt = processAt ?? DateTime.UtcNow,
                CreateTime = DateTime.UtcNow,
                UpdateTime = DateTime.UtcNow
            };
            await _session.SaveAsync(queueItem, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(e, e.Message);
        }
    }
    
    public async Task<QueueEntity?> GetById(
        long id,
        CancellationToken cancellationToken = default
    )
    {
        var query = _session.Query<QueueEntity>()
            .Where(item => item.Id == id);
        return await query
            .OrderBy(item => item.CreateTime)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
    
    public async Task<QueueEntity?> GetTop(
        QueueChannel? channel = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var result = await _session.CreateSQLQuery(@"SELECT * FROM fn_queue_get_top(:channel)")
                .AddEntity(typeof(QueueEntity))
                .SetParameter("channel", channel ?? QueueChannel.Default)
                .SetFlushMode(FlushMode.Always)
                .SetResultTransformer(new RootEntityResultTransformer())
                .ListAsync<QueueEntity>();
            var entity = result?.FirstOrDefault();
            if (entity != null)
                await _session.RefreshAsync(entity);
            return entity;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
        return null;
    }

    public async Task MarkAsProcessed(
        QueueEntity item,
        string? error = null,
        CancellationToken cancellationToken = default
    )
    {
        if (item.Status != QueueStatus.InProcess)
        {
            throw new Exception("This item already processed");
        }

        if (string.IsNullOrEmpty(error))
        {
            item.Status = QueueStatus.Success;
        }
        else
        {
            item.Error = error;
            item.Status = QueueStatus.Fail;
        }
        await _session.SaveAsync(item, cancellationToken);
    }

    public async Task<int> CompleteAllPending(CancellationToken cancellationToken = default)
    {
        return await _session.Query<QueueEntity>()
            .UpdateAsync(item => new {
                Status = QueueStatus.Success
            }, cancellationToken: cancellationToken);
    }
    
    public async Task UpdateProcessAtForPending()
    {
        await _session.Query<QueueEntity>()
            .Where(x => x.Status == QueueStatus.Pending)
            .UpdateBuilder()
            .Set(x => x.ProcessAt, DateTime.UtcNow)
            .UpdateAsync();

    }
    
    public void Dispose()
    {
        Flush();
        _session.Dispose();
    }
    
    public void Flush()
    {
        _session.Flush();
    }
}
