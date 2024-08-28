using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Web.Store.Tag;

namespace TimeTracker.Web.Pages.Dashboard.Tag.Parts;

public partial class UpdateTagModal
{
    [CascadingParameter] 
    public MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public TagDto Tag { get; set; }
    
    private UpdateRequest model = new();
    private bool _isLoading = false;
    private bool _isValid = false;
    private MudForm _form;

    private MudColor _mudColor
    {
        get => string.IsNullOrEmpty(model.Color) ? new MudColor("#ffffff") : new MudColor(model.Color);
        set => model.Color = value?.Value;
    }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.Fill(Tag);
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
            model.Color = _mudColor.Value;
            var responseDto = await ApiService.TagUpdateAsync(model);
            if (responseDto != null)
            {
                Dispatcher.Dispatch(new SetListItemAction(responseDto));
                await ToastService.ShowInfo("Tag was updated");
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
    }

    private void OnCloseModal()
    {
        MudDialog.Close();
    }
}
