using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Tasks;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Pages.Dashboard.Shared.Tasks;

public partial class AddTaskModalForm
{
    [Parameter]
    public long? TimeEntryId { get; set; }
    
    [Parameter]
    public long? TaskListId { get; set; }
    
    [Parameter]
    public TaskStatus? TaskStatus { get; set; }
    
    [Parameter]
    public DateTime? EndTime { get; set; }
    
    [CascadingParameter] 
    MudDialogInstance MudDialog { get; set; }

    private AddRequest model = new();
    private bool _isLoading = false;
    private bool _isValid = false;
    private MudForm _form;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.TimeEntryId = TimeEntryId;
        if (TaskListId.HasValue)
        {
            model.TaskListId = TaskListId.Value;
        }
        if (TaskStatus.HasValue)
        {
            model.Status = TaskStatus.Value;
        }
        if (EndTime.HasValue)
        {
            model.EndTime = EndTime.Value;
        }
    }

    private async Task Submit()
    {
        _form.Validate();
        if (!_form.IsValid)
        {
            return;
        }

        _isLoading = true;
        try
        {
            var responseDto = await ApiService.TasksAddAsync(model);
            if (responseDto != null)
            {
                OnCloseModal();
                Dispatcher.Dispatch(new SetListItemAction(responseDto));
                Dispatcher.Dispatch(new SetOverdueTasksListItemAction(responseDto));
                Dispatcher.Dispatch(new TimeTracker.Web.Store.TimeEntry.SetSelectedPageAction(1));
                Dispatcher.Dispatch(new TimeTracker.Web.Store.TimeEntry.LoadListAction());
            }
        }
        catch (Exception e)
        {
            await ToastService.ShowError(e.Message);
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();    
    }

    private void OnCloseModal()
    {
        MudDialog.Close();
    }

    private async Task OnKeyUp(KeyboardEventArgs arg)
    {
        if (arg.Code == "Enter")
        {
            await Submit();
        }
        await Task.CompletedTask;
    }
}
