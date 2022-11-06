using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public class WorkspaceSeeder: IWorkspaceSeeder
{
    private readonly IDataFactory<WorkspaceEntity> _workspaceFactory;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public WorkspaceSeeder(
        IDataFactory<WorkspaceEntity> workspaceFactory,
        IWorkspaceDao workspaceDao,
        IWorkspaceAccessService workspaceAccessService
    )
    {
        _workspaceFactory = workspaceFactory;
        _workspaceDao = workspaceDao;
        _workspaceAccessService = workspaceAccessService;
    }

    public async Task<ICollection<WorkspaceEntity>> CreateSeveralAsync(UserEntity user, int count = 1)
    {
        var result = new List<WorkspaceEntity>();
        for (int i = 0; i < count; i++)
        {
            var fakeEntry = _workspaceFactory.Generate();
            var entry = await _workspaceDao.CreateWorkspaceAsync(user, fakeEntry.Name);
            await _workspaceAccessService.ShareAccessAsync(entry, user, MembershipAccessType.Owner);
            result.Add(entry);
        }

        return result;
    }
}
