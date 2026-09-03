using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TimeTracker.Api.Shared.Dto.Entity.Task;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Modals.EditTask.Components.Checklist;

public partial class ChecklistItem
{
    private ElementReference _editInputRef;
    private bool _isEditing;
    private bool _shouldFocusInput;
    private string _editingTitle = string.Empty;

    [Parameter]
    public required TaskSubTaskDto SubTask { get; set; }

    [Parameter]
    public bool IsDragging { get; set; }

    [Parameter]
    public bool IsDropTarget { get; set; }

    [Parameter]
    public EventCallback<TaskSubTaskDto> OnToggle { get; set; }

    [Parameter]
    public EventCallback<TaskSubTaskDto> OnDelete { get; set; }

    [Parameter]
    public EventCallback<(TaskSubTaskDto SubTask, string NewTitle)> OnTitleChanged { get; set; }

    [Parameter]
    public EventCallback OnDragStartCallback { get; set; }

    [Parameter]
    public EventCallback OnDragOverCallback { get; set; }

    [Parameter]
    public EventCallback OnDropCallback { get; set; }

    [Parameter]
    public EventCallback OnDragEndCallback { get; set; }

    protected override async System.Threading.Tasks.Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (_shouldFocusInput)
        {
            _shouldFocusInput = false;
            try
            {
                await _editInputRef.FocusAsync();
            }
            catch
            {
                // ignore
            }
        }
    }

    private async System.Threading.Tasks.Task ToggleCompleted()
    {
        if (_isEditing)
        {
            return;
        }
        await OnToggle.InvokeAsync(SubTask);
    }

    private void StartEdit()
    {
        _editingTitle = SubTask.Title;
        _isEditing = true;
        _shouldFocusInput = true;
    }

    private void HandleEditInput(ChangeEventArgs e)
    {
        _editingTitle = e.Value?.ToString() ?? string.Empty;
    }

    private async System.Threading.Tasks.Task HandleEditKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await SaveEdit();
        }
        else if (e.Key == "Escape")
        {
            _isEditing = false;
        }
    }

    private async System.Threading.Tasks.Task SaveEdit()
    {
        if (!_isEditing)
        {
            return;
        }

        var trimmed = _editingTitle.Trim();
        _isEditing = false;

        if (!string.IsNullOrWhiteSpace(trimmed) && trimmed != SubTask.Title)
        {
            await OnTitleChanged.InvokeAsync((SubTask, trimmed));
        }
    }

    private async System.Threading.Tasks.Task Delete()
    {
        await OnDelete.InvokeAsync(SubTask);
    }

    private async System.Threading.Tasks.Task HandleDragStart()
    {
        if (_isEditing) return;
        await OnDragStartCallback.InvokeAsync();
    }

    private async System.Threading.Tasks.Task HandleDragOver()
    {
        if (_isEditing) return;
        await OnDragOverCallback.InvokeAsync();
    }

    private async System.Threading.Tasks.Task HandleDrop()
    {
        if (_isEditing) return;
        await OnDropCallback.InvokeAsync();
    }

    private async System.Threading.Tasks.Task HandleDragEnd()
    {
        await OnDragEndCallback.InvokeAsync();
    }
}
