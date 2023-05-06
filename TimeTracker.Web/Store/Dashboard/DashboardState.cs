using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.Dashboard;

public record struct MyTasksState(
    ICollection<TaskDto> List,
    int TotalCount,
    int TotalPages,
    bool HasMoreItems,
    bool IsLoading
);

[FeatureState]
public record DashboardState
{
    public MyTasksState MyTasks = new ()
    {
        List = new List<TaskDto>()
    };
}
