using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Security;

public class SecurityManager: ISecurityManager
{
    private readonly ITimeEntryDao _timeEntryDao;

    public SecurityManager(ITimeEntryDao timeEntryDao)
    {
        _timeEntryDao = timeEntryDao;
    }

    public async Task<bool> HasAccess<TEntity>(AccessLevel accessLevel, UserEntity user, TEntity entity)
    {
        if (entity is TimeEntryEntity entryEntity)
        {
            return await _timeEntryDao.HasAccessAsync(user, entryEntity);
        }

        throw new NotImplementedException($"Security checking not implemented for {entity?.GetTypeName()}");
    }
}
