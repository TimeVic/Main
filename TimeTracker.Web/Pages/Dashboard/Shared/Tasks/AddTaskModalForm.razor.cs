using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Store.Tasks;

namespace TimeTracker.Web.Pages.Dashboard.Shared.Tasks;

public partial class AddTaskModalForm
{
    [Parameter]
    public long? TimeEntryId { get; set; }
    
    [Parameter]
    public long? TaskListId { get; set; }
    
    private RadzenTemplateForm<AddRequest> _form;

    private AddRequest model = new();
    private bool _isLoading = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.TimeEntryId = TimeEntryId;
        if (TaskListId.HasValue)
        {
            model.TaskListId = TaskListId.Value;
        }
    }

    private void HandleSubmit(AddRequest request)
    {
        InvokeAsync(async () =>
        {
            _isLoading = true;
            try
            {
                var responseDto = await ApiService.TasksAddAsync(model);
                if (responseDto != null)
                {
                    Dispatcher.Dispatch(new SetListItemAction(responseDto));
                    Dispatcher.Dispatch(new TimeTracker.Web.Store.TimeEntry.LoadListAction(0));
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
        DialogService.Close();
    }
}
