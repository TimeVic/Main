using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Security;

public class SecurityManager: ISecurityManager
{
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IProjectDao _projectDao;

    public SecurityManager(
        ITimeEntryDao timeEntryDao,
        IProjectDao projectDao
    )
    {
        _timeEntryDao = timeEntryDao;
        _projectDao = projectDao;
    }

    public async Task<bool> HasAccess<TEntity>(AccessLevel accessLevel, UserEntity user, TEntity entity)
    {
        if (entity is TimeEntryEntity entryEntity)
        {
            return await _timeEntryDao.HasAccessAsync(user, entryEntity);
        }
        if (entity is ProjectEntity projectEntity)
        {
            return await _projectDao.HasAccessAsync(user, projectEntity);
        }

        throw new NotImplementedException($"Security checking not implemented for {entity?.GetTypeName()}");
    }
}
