using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Tasks;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Pages.Dashboard.Shared.Tasks;

public partial class AddTaskModalForm
{
    public class Parameters
    {
        public long? TimeEntryId { get; set; }
        public long? TaskListId { get; set; }
        public TaskStatus? TaskStatus { get; set; }
        public DateTime? EndTime { get; set; }
    }
    
    [Parameter]
    public required Parameters Content { get; set; }
    
    [CascadingParameter] 
    public required FluentDialog? Dialog { get; set; }

    private AddRequest model = new();
    private bool _isLoading = false;
    private bool _isValid = false;
    private EditForm _form;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.TimeEntryId = Content.TimeEntryId;
        if (Content.TaskListId.HasValue)
        {
            model.TaskListId = Content.TaskListId.Value;
        }
        if (Content.TaskStatus.HasValue)
        {
            model.Status = Content.TaskStatus.Value;
        }
        if (Content.EndTime.HasValue)
        {
            model.EndTime = Content.EndTime.Value;
        }
    }

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
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
            ToastService.ShowError(e.Message);
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();    
    }

    private void OnCloseModal()
    {
        Dialog?.CloseAsync();
    }
}
