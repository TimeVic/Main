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
    private readonly ISession _session;

    public QueueDao(IDbSessionProvider sessionProvider, ILogger<IQueueDao> logger)
    {
        _logger = logger;
        _session = sessionProvider.CreateSession();
    }

    public async Task Push(
        object context,
        QueueChannel channel = QueueChannel.Default,
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
                ContextType = typeString,
                ContextData = JsonHelper.SerializeToString(context),
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
        using var transaction = _session.BeginTransaction();

        try
        {
            var query = _session.Query<QueueEntity>()
                .Where(item => item.Status == QueueStatus.Pending);

            if (channel.HasValue)
            {
                query = query.Where(item => item.Channel == channel);
            }

            var result = await query
                .OrderBy(item => item.CreateTime)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (result != null)
            {
                result.Status = QueueStatus.InProcess;
                await _session.SaveAsync(result, cancellationToken);    
            }

            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
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
    
    public void Dispose()
    {
        _session.Flush();
        if (_session.IsOpen)
        {
            _session.Close();    
        }
    }
}
