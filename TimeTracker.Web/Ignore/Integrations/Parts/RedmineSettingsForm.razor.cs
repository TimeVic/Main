using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Web.Pages.Dashboard.Integrations.Parts;

public partial class RedmineSettingsForm
{
    [Parameter]
    public WorkspaceSettingsRedmineDto? Value { get; set; }

    [Parameter]
    public EventCallback<WorkspaceSettingsRedmineDto> ValueChanged { get; set; }
    
    [Inject]
    public IState<AuthState> _authState { get; set; }
    
    private SetRedmineSettingsRequest _model = new();
    private bool _isLoading = false;
    private FluentEditForm _form;
    private bool _isValid = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (Value != null)
        {
            _model.Fill(Value);    
        }
    }
    
    private async Task HandleSubmit()
    {
        _form.Validate();
        if (!_form.IsValid)
        {
            return;
        }
        
        _isLoading = true;
        try
        {
            var responseDto = await ApiService.WorkspaceSetRedmineIntegrationSettingsAsync(_model);
            if (responseDto != null)
            {
                await ToastService.ShowInfo("The settings was saved");
                if (!responseDto.IsActive)
                {
                    await ToastService.ShowWarning("Integration to Redmine was not activated. Please check the settings");
                }
                else
                {
                    await ToastService.ShowInfo("Integration to Redmine is activated");
                }
            }
        }
        catch (Exception)
        {
            await ToastService.ShowError("Settings saving error");
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }
}
