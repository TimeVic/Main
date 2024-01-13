using Microsoft.AspNetCore.Components;
using MudBlazor;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker;
using TimeTracker.Web.Store.GoalsTracker;

namespace TimeTracker.Web.Pages.Dashboard.GoalsTracker.Parts;

public partial class AddGoalModalForm
{
    [CascadingParameter] 
    MudDialogInstance MudDialog { get; set; }

    private AddGoalRequest model = new();
    private bool _isLoading = false;
    private bool _isValid = false;
    private MudForm _form;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
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
            Dispatcher.Dispatch(new SetGoalsTrackerItemAction(new GoalsTrackerItemDto()
            {
                Id = new Random().Next(100000, 10000000),
                Name = model.Name,
                NumberOfTimes = model.NumberOfTimes
            }));
            OnCloseModal();
            
            // var responseDto = await ApiService.TasksAddAsync(model);
            // if (responseDto != null)
            // {
            //     Dispatcher.Dispatch(new SetListItemAction(responseDto));
            //     Dispatcher.Dispatch(new SetOverdueTasksListItemAction(responseDto));
            //     Dispatcher.Dispatch(new TimeTracker.Web.Store.TimeEntry.SetSelectedPageAction(1));
            //     Dispatcher.Dispatch(new TimeTracker.Web.Store.TimeEntry.LoadListAction());
            //     OnCloseModal();
            // }
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
}
