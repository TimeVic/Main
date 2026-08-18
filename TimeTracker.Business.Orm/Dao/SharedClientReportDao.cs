using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public class SharedClientReportDao : ISharedClientReportDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public SharedClientReportDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<SharedClientReportEntity?> GetByClientIdAsync(Guid clientId)
    {
        return await _sessionProvider.CurrentSession.Query<SharedClientReportEntity>()
            .Fetch(item => item.Client)
            .ThenFetch(item => item.Workspace)
            .ThenFetch(item => item.CreatedUser)
            .ThenFetch(item => item.Language)
            .FirstOrDefaultAsync(item => item.Client.Id == clientId);
    }

    public async Task<SharedClientReportEntity?> GetByTokenAsync(string token)
    {
        return await _sessionProvider.CurrentSession.Query<SharedClientReportEntity>()
            .Fetch(item => item.Client)
            .ThenFetch(item => item.Workspace)
            .ThenFetch(item => item.CreatedUser)
            .ThenFetch(item => item.Language)
            .FirstOrDefaultAsync(item => item.Token == token);
    }

    public async Task<SharedClientReportEntity> CreateAsync(ClientEntity client, string token)
    {
        var report = new SharedClientReportEntity
        {
            Client = client,
            Token = token,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _sessionProvider.CurrentSession.SaveAsync(report);
        return report;
    }

    public Task SaveAsync(SharedClientReportEntity report)
    {
        return _sessionProvider.CurrentSession.SaveAsync(report);
    }
}
