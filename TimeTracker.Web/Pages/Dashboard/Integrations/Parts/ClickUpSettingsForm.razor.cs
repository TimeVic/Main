﻿using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
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

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (Value != null)
        {
            _model.Fill(Value);    
        }

        Debug.Log(_model);
        _model.WorkspaceId = _authState.Value.Workspace.Id;
    }
    
    private async Task HandleSubmit()
    {
        _isLoading = true;
        try
        {
            var responseDto = await ApiService.WorkspaceSetClickUpIntegrationSettingsAsync(_model);
            if (responseDto != null)
            {
                NotificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "The settings was saved"
                });
                if (!responseDto.IsActive)
                {
                    NotificationService.Notify(new NotificationMessage()
                    {
                        Severity = NotificationSeverity.Warning,
                        Summary = "Integration to ClickUp was not activated. Please check the settings"
                    });
                }
                else
                {
                    NotificationService.Notify(new NotificationMessage()
                    {
                        Severity = NotificationSeverity.Info,
                        Summary = "Integration to ClickUp is activated"
                    });
                }
            }
        }
        catch (Exception)
        {
            NotificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Error,
                Summary = "Settings saving error"
            });
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }
}
