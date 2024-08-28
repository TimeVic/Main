using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker;
using TimeTracker.Web.Store.GoalsTracker;

namespace TimeTracker.Web.Pages.Dashboard.GoalsTracker.Parts;

public partial class AddGoalModalForm
{
    [CascadingParameter] 
    FluentDialog MudDialog { get; set; }

    private CreateItemRequest model = new();
    private bool _isLoading = false;
    private bool _isValid = false;
    private FluentEditForm _form;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private async Task Submit()
    {
        if (!_form.EditContext.Validate())
        {
            return;
        }

        _isLoading = true;
        try
        {
            Dispatcher.Dispatch(new CreateTrackerItemAction(model.Name, model.NumberOfTimes));
            OnCloseModal();
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
