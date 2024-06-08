using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Pages.Dashboard.Integrations.Parts;

public partial class ClickUpSettingsForm
{
    [Parameter]
    public WorkspaceSettingsClickUpDto? Value { get; set; }

    [Parameter]
    public EventCallback<WorkspaceSettingsClickUpDto> ValueChanged { get; set; }
    
    [Inject]
    public IState<AuthState> _authState { get; set; }
    
    private SetClickUpSettingsRequest _model = new();
    private bool _isLoading = false;
    private bool _isValid = false;
    private MudForm _form;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (Value != null)
        {
            _model.Fill(Value);    
        }

        _model.WorkspaceId = _authState.Value.Workspace.Id;
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
            var responseDto = await ApiService.WorkspaceSetClickUpIntegrationSettingsAsync(_model);
            if (responseDto != null)
            {
                await ToastService.ShowInfo("The settings was saved");
                if (!responseDto.IsActive)
                {
                    await ToastService.ShowWarning("Integration to ClickUp was not activated. Please check the settings");
                }
                else
                {
                    await ToastService.ShowInfo("Integration to ClickUp is activated");
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
