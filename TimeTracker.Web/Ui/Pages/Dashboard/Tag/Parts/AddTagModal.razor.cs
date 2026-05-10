using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Web.Store.Tag;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Tag.Parts;

public partial class AddTagModal
{
    [CascadingParameter] 
    FluentDialog MudDialog { get; set; }

    private AddRequest model = new();
    private bool _isLoading = false;
    private bool _isValid = false;
    private FluentEditForm _form;

    private Color _mudColor
    {
        get => string.IsNullOrEmpty(model.Color) ? new MudColor("#ffffff") : new MudColor(model.Color);
        set => model.Color = value.Value;
    }

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
            var responseDto = await ApiService.TagAddAsync(model);
            if (responseDto != null)
            {
                Microsoft.AspNetCore.Components.Dispatcher.Dispatch(new SetListItemAction(responseDto));
                await Microsoft.FluentUI.AspNetCore.Components.ToastService.ShowInfo(DashboardLocalizer["TagAdded"].Value);
                OnCloseModal();
            }
        }
        catch (Exception e)
        {
            await Microsoft.FluentUI.AspNetCore.Components.ToastService.ShowError(e.Message);
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
