using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
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
    }

    private void Submit()
    {
        _form.Validate();
        if (!_form.IsValid)
        {
            return;
        }

        InvokeAsync(async () =>
        {
            _isLoading = true;
            try
            {
                var responseDto = await ApiService.TasksAddAsync(model);
                if (responseDto != null)
                {
                    Dispatcher.Dispatch(new SetListItemAction(responseDto));
                    Dispatcher.Dispatch(new TimeTracker.Web.Store.TimeEntry.SetSelectedPageAction(1));
                    Dispatcher.Dispatch(new TimeTracker.Web.Store.TimeEntry.LoadListAction());
                    OnCloseModal();
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
        });
    }

    private void OnCloseModal()
    {
        MudDialog.Close();
    }
}
