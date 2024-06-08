using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.ExternalClients.Dto;

namespace TimeTracker.Business.Services.ExternalClients.Redmine;

public partial class RedmineClient
{
    protected override Task<TaskEntity?> CreateOrUpdateTimeEntryTaskAsync(
        TimeEntryEntity timeEntry,
        TaskListEntity taskListEntity,
        string externalTaskId
    )
    {
        throw new NotImplementedException();
    }

    protected override Task<TaskEntity> CreateOrUpdateTimeEntryTaskAsync(
        TaskListEntity taskList,
        UserEntity user,
        string externalTaskId
    )
    {
        throw new NotImplementedException();
    }
}
