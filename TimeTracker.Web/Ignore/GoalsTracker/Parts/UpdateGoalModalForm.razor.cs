using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker;
using TimeTracker.Client.Core.Store.GoalsTracker;

namespace TimeTracker.Web.Pages.Dashboard.GoalsTracker.Parts;

public partial class UpdateGoalModalForm
{
    [Parameter] 
    public GoalsTrackerItemDto Item { get; set; }
    
    [CascadingParameter] 
    MudDialogInstance MudDialog { get; set; }

    private UpdateItemRequest model = new();
    private bool _isLoading = false;
    private bool _isValid = false;
    private MudForm _form;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.GoalsTrackerItemId = Item.Id;
        model.Name = Item.Name;
        model.NumberOfTimes = Item.NumberOfTimes;
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
            Dispatcher.Dispatch(new UpdateTrackerItemAction(model));
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
