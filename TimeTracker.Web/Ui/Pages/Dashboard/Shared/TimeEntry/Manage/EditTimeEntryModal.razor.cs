using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.TimeEntry.Manage;

public partial class EditTimeEntryModal: IDisposable
{
    [Parameter]
    public required TimeEntryDto Entry { get; set; }
    
    [Parameter]
    public EventCallback OnClose { get; set; }
    
    private TimeEntryDto _model = new();
    private EditForm _form;
    private LumexModal modal;
    private EditContext _editContext;

    protected override async Task OnInitializedAsync()
    {
        _editContext = new EditContext(_model);
        _model.UpdateFrom(Entry);
        await base.OnInitializedAsync();
        _editContext.OnFieldChanged += OnFormFieldChanged;
    }

    public void Dispose()
    {
        _editContext?.OnFieldChanged -= OnFormFieldChanged;
    }
    
    private void OnFormFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        
    }

    private async Task UpdateTimeEntry()
    {
        if (_editContext.Validate())
        {
            Dispatcher.Dispatch(new SaveTimeEntryAction(_model, true));
        }
        await Task.CompletedTask;
    }

    private void OnCloseModal()
    {
        OnClose.InvokeAsync();
    }

    private async Task OnProjectSelected(ProjectDto project)
    {
        _model.Project = project;
        await UpdateTimeEntry();
    }

    private async Task ClearProject()
    {
        _model.Project = null;
        await UpdateTimeEntry();
    }
}
