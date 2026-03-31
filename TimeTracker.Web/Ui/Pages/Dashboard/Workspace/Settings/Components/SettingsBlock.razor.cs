using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Common;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Workspace.Settings.Components;

public partial class SettingsBlock
{
    [Inject]
    public IState<AuthState> _authState { get; set; }
    
    private WorkspaceDto? _workspace => _authState.Value.Workspace;
}
