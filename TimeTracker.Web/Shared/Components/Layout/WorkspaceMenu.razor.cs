﻿using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Common.Actions;
using TimeTracker.Web.Store.Workspace;

namespace TimeTracker.Web.Shared.Components.Layout;

public partial class WorkspaceMenu
{
    [Inject]
    public IState<WorkspaceState> _workpsaceState { get; set; }
    
    [Inject]
    public IState<AuthState> _authState { get; set; }
    
    [Inject]
    public UrlService _urlService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private void OnClickToMenuItem(string? idString)
    {
        if (string.IsNullOrEmpty(idString))
        {
            // Clicked on selected item
            return;
        }

        var workspace = _workpsaceState.Value.List.First(item => item.Id.ToString() == idString);
        if (workspace.Id == _authState.Value.Workspace.Id)
        {
            return;
        }

        _urlService.NavigateToChangeWorkspace(workspace.Id, SiteUrl.Dashboard_TimeEntry);
    }
    
    private void NavigateToEditPage()
    {
        NavigationManager.NavigateTo(SiteUrl.Workspace_List);
    }
}
