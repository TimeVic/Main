using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Parts;

public partial class UpdateWorkspaceForm
{
    [Inject]
    public IState<AuthState> _authState { get; set; }
    
    private WorkspaceDto? _workspace => _authState.Value.Workspace;
    private IReadOnlyCollection<TimeZoneInfo> _timeZones;
    
    private UpdateRequest model = new();
    private EditForm _form;
    
    protected override async Task OnInitializedAsync()
    {
        _timeZones = TimeZoneInfo.GetSystemTimeZones();
        if (_workspace != null)
            model.Fill(_workspace);
        await base.OnInitializedAsync();
    }

    private Task OnSave()
    {
        throw new NotImplementedException();
    }
}
