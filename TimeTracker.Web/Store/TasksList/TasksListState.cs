using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;

namespace TimeTracker.Web.Store.TasksList;

[FeatureState]
public record TasksListState
{
    public Guid? SelectedTaskListId { get; set; }
    
    public ICollection<TaskListDto> List { get; set; } = new List<TaskListDto>();
    
    public int TotalCount { get; set; }
    
    public int TotalPages { get; set; }
    
    public bool HasMoreItems { get; set; }
    
    public bool IsListLoading { get; set; }

    public bool IsLoaded { get; set; } = false;
    
    public TaskListDto? SelectedTaskList => List.FirstOrDefault(item => item.Id == SelectedTaskListId);
}
