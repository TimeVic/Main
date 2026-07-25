using Autofac;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Workspace;
using TimeTracker.WorkerServices.Core;

namespace TimeTracker.WorkerServices.Services.Workspace;

internal class WorkspaceHardDeletionHostedService : ABackgroundService
{
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IWorkspaceDeletionService _workspaceDeletionService;
    private readonly ILogger<WorkspaceHardDeletionHostedService> _workspaceLogger;

    public WorkspaceHardDeletionHostedService() : base()
    {
        _workspaceDao = DiScope.Resolve<IWorkspaceDao>();
        _workspaceDeletionService = DiScope.Resolve<IWorkspaceDeletionService>();
        _workspaceLogger = DiScope.Resolve<ILogger<WorkspaceHardDeletionHostedService>>();
        ServiceName = nameof(WorkspaceHardDeletionHostedService);
    }

    protected override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        var workspaceIds = (await _workspaceDao.GetDeletedBeforeAsync(DateTime.UtcNow.AddDays(-30)))
            .Select(item => item.Id)
            .ToList();
        DbSessionProvider.CloseCurrentSession();

        foreach (var workspaceId in workspaceIds)
        {
            try
            {
                DbSessionProvider.SetTransactional();
                var workspace = await _workspaceDao.GetDeletedByIdAsync(workspaceId);
                if (workspace == null)
                {
                    await DbSessionProvider.RollbackCommitAsync(cancellationToken);
                    continue;
                }

                await _workspaceDeletionService.HardDeleteAsync(workspace, cancellationToken);
                await DbSessionProvider.PerformCommitAsync(true, cancellationToken);
            }
            catch (Exception exception)
            {
                await DbSessionProvider.RollbackCommitAsync(cancellationToken);
                _workspaceLogger.LogError(exception, "Failed to hard delete workspace {WorkspaceId}", workspaceId);
            }
        }
    }

    protected override string GetCrontabExpression() => "0 2 * * *";
}
