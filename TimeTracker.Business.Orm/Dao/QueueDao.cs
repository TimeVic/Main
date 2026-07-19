using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public class QueueDao: IQueueDao
{
    private readonly ILogger<IQueueDao> _logger;
    private readonly IDbSessionProvider _sessionProvider;
    private readonly ISession _session;

    private ISession Session => _session;

    public QueueDao(IDbSessionProvider sessionProvider, ILogger<IQueueDao> logger)
    {
        _sessionProvider = sessionProvider;
        _logger = logger;
        _session = sessionProvider.CreateSession(FlushMode.Manual);
    }

    public async Task Push(
        object context,
        QueueChannel channel = QueueChannel.Default,
        DateTime? processAt = null,
        QueuePriority? priority = null,
        CancellationToken cancellationToken = default
    )
    {
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
                // Account for small clock drift between the application and database hosts.
                ProcessAt = processAt ?? DateTime.UtcNow.AddSeconds(-1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await Session.SaveAsync(queueItem, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }
    
    public async Task<QueueEntity?> GetById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var query = Session.Query<QueueEntity>()
            .Where(item => item.Id == id);
        return await query
            .OrderBy(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
    
    public async Task<QueueEntity?> GetTop(
        QueueChannel? channel = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            using var session = _sessionProvider.CreateSession(FlushMode.Manual);

            var result = await session.CreateSQLQuery(@"SELECT queue.* FROM fn_queue_get_top(:channel) queue")
                .AddEntity("queue", typeof(QueueEntity))
                .SetParameter("channel", (int)(channel ?? QueueChannel.Default))
                .ListAsync<QueueEntity>(cancellationToken);

            var queueItem = result.FirstOrDefault();
            if (queueItem == null)
            {
                return null;
            }

            return Session.Merge(queueItem);
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
        ArgumentNullException.ThrowIfNull(item);

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
        Session.Merge(item);
        await Session.FlushAsync(cancellationToken);
    }

    public async Task<int> CompleteAllPending(CancellationToken cancellationToken = default)
    {
        return await Session.Query<QueueEntity>()
            .UpdateBuilder()
            .Set(x => x.Status, QueueStatus.Success)
            .UpdateAsync(cancellationToken);
    }
    
    public async Task UpdateProcessAtForPending()
    {
        await Session.Query<QueueEntity>()
            .Where(x => x.Status == QueueStatus.Pending)
            .UpdateBuilder()
            .Set(x => x.ProcessAt, DateTime.UtcNow.AddSeconds(-1))
            .UpdateAsync();

    }
    
    public void Clear()
    {
        Session.Clear();
    }
    
    public void Dispose()
    {
        if (_session.IsOpen)
        {
            _session.Dispose();
        }
    }
    
    public async Task Flush()
    {
        await Session.FlushAsync();
    }
}
