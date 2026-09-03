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

    private int Percentage => TotalCount > 0
        ? (int)Math.Round((double)CompletedCount / TotalCount * 100)
        : 0;

    private TaskSubTaskDto? _draggingItem;
    private TaskSubTaskDto? _dragOverItem;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        NormalizePositions();
    }

    private void NormalizePositions()
    {
        if (!SubTasks.Any()) return;

        var ordered = SubTasks.OrderBy(x => x.PositionIndex).ThenBy(x => x.CreatedAt).ToList();
        var isSequential = true;
        for (var i = 0; i < ordered.Count; i++)
        {
            if (ordered[i].PositionIndex != i)
            {
                isSequential = false;
                break;
            }
        }

        if (!isSequential)
        {
            for (var i = 0; i < ordered.Count; i++)
            {
                ordered[i].PositionIndex = i;
            }
        }
    }

    private void HandleAdd(string title)
    {
        if (TaskId == Guid.Empty || string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        NormalizePositions();
        var nextPosition = SubTasks.Any() ? SubTasks.Max(x => x.PositionIndex) + 1 : 0;

        var tempSubTask = new TaskSubTaskDto
        {
            Id = Guid.NewGuid(),
            TaskId = TaskId,
            Title = title,
            IsCompleted = false,
            PositionIndex = nextPosition,
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
                var list = SubTasks.ToList();
                var index = list.FindIndex(x => x.Id == tempSubTask.Id);
                if (index >= 0)
                {
                    SubTasks.Remove(tempSubTask);
                }
                created.PositionIndex = nextPosition;
                SubTasks.Add(created);
                NormalizePositions();
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
        NormalizePositions();
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

    private void OnDragStart(TaskSubTaskDto item)
    {
        _draggingItem = item;
    }

    private void OnDragOver(TaskSubTaskDto item)
    {
        if (_draggingItem == null || _draggingItem.Id == item.Id || _dragOverItem?.Id == item.Id)
        {
            return;
        }
        _dragOverItem = item;
    }

    private void OnDrop(TaskSubTaskDto targetItem)
    {
        if (_draggingItem == null || _draggingItem.Id == targetItem.Id)
        {
            ResetDragState();
            return;
        }

        var ordered = Items;
        var fromIndex = ordered.IndexOf(_draggingItem);
        var toIndex = ordered.IndexOf(targetItem);

        if (fromIndex < 0 || toIndex < 0)
        {
            ResetDragState();
            return;
        }

        ordered.RemoveAt(fromIndex);
        ordered.Insert(toIndex, _draggingItem);

        var positions = new Dictionary<Guid, int>();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].PositionIndex = i;
            positions[ordered[i].Id] = i;
        }

        ResetDragState();
        StateHasChanged();

        Dispatcher.Dispatch(new UpdateSubTaskPositionsAction(TaskId, positions));
    }

    private void OnDragEnd()
    {
        ResetDragState();
    }

    private void ResetDragState()
    {
        _draggingItem = null;
        _dragOverItem = null;
    }
}
