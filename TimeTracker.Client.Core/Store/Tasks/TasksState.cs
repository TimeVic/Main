using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;

using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Client.Core.Store.Tasks;

[FeatureState]
public record TasksState
{
    public ICollection<TaskDto> List { get; set; } = new List<TaskDto>();
    public int TotalCount { get; set; }
    
    public int TotalPages { get; set; }
    
    public bool HasMoreItems { get; set; }
    
    public bool IsListLoading { get; set; }

    public bool IsTaskSaving { get; set; }

    public HashSet<TaskStatus> ExpandedStatuses { get; init; } = [TaskStatus.InProgress, TaskStatus.ToDo];
    
    public bool IsLoaded { get; set; } = false;

    public GetListFilterRequest Filter { get; set; } = new();
    
    #region Overdue
    
    public ICollection<TaskDto> OverdueList { get; set; } = new List<TaskDto>();
    
    public bool IsOverdueListLoading { get; set; }
    
    #endregion
}
