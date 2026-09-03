using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Core.Store.Tasks;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Modals.EditTask.Components.Checklist;

public partial class ChecklistBlock
{
    [Parameter]
    public Guid TaskId { get; set; }

    [Parameter]
    public ICollection<TaskSubTaskDto> SubTasks { get; set; } = new List<TaskSubTaskDto>();

    [Parameter]
    public string Class { get; set; } = string.Empty;

    private List<TaskSubTaskDto> Items => SubTasks.OrderBy(x => x.PositionIndex).ThenBy(x => x.CreatedAt).ToList();

    private int TotalCount => SubTasks.Count;

    private int CompletedCount => SubTasks.Count(x => x.IsCompleted);

    private void HandleAdd(string title)
    {
        if (TaskId == Guid.Empty || string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        var tempSubTask = new TaskSubTaskDto
        {
            Id = Guid.NewGuid(),
            TaskId = TaskId,
            Title = title,
            IsCompleted = false,
            PositionIndex = SubTasks.Count,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        SubTasks.Add(tempSubTask);
        StateHasChanged();

        Dispatcher.Dispatch(new AddSubTaskAction(
            TaskId,
            title,
            OnSuccess: created =>
            {
                var index = SubTasks.ToList().FindIndex(x => x.Id == tempSubTask.Id);
                if (index >= 0)
                {
                    SubTasks.Remove(tempSubTask);
                }
                SubTasks.Add(created);
                InvokeAsync(StateHasChanged);
            }
        ));
    }

    private void HandleToggle(TaskSubTaskDto subTask)
    {
        var newCompleted = !subTask.IsCompleted;
        subTask.IsCompleted = newCompleted;
        StateHasChanged();

        Dispatcher.Dispatch(new UpdateSubTaskAction(
            TaskId,
            subTask.Id,
            subTask.Title,
            newCompleted,
            OnSuccess: updated =>
            {
                subTask.IsCompleted = updated.IsCompleted;
                subTask.Title = updated.Title;
                InvokeAsync(StateHasChanged);
            }
        ));
    }

    private void HandleTitleChanged((TaskSubTaskDto SubTask, string NewTitle) args)
    {
        var (subTask, newTitle) = args;
        subTask.Title = newTitle;
        StateHasChanged();

        Dispatcher.Dispatch(new UpdateSubTaskAction(
            TaskId,
            subTask.Id,
            newTitle,
            subTask.IsCompleted,
            OnSuccess: updated =>
            {
                subTask.Title = updated.Title;
                InvokeAsync(StateHasChanged);
            }
        ));
    }

    private void HandleDelete(TaskSubTaskDto subTask)
    {
        var wasCompleted = subTask.IsCompleted;
        SubTasks.Remove(subTask);
        StateHasChanged();

        Dispatcher.Dispatch(new DeleteSubTaskAction(
            TaskId,
            subTask.Id,
            wasCompleted,
            OnSuccess: () =>
            {
                InvokeAsync(StateHasChanged);
            }
        ));
    }
}
