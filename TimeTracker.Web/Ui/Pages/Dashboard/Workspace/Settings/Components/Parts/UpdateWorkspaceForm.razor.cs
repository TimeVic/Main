using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.List;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Workspace;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Parts;

public partial class UpdateWorkspaceForm
{
    [Inject]
    public IState<AuthState> _authState { get; set; }
    
    [Inject]
    public IState<WorkspaceState> _workspaceState { get; set; }
    
    private WorkspaceDto? _workspace;
    private IReadOnlyCollection<TimeZoneInfo> _timeZones;
    
    private UpdateRequest model = new();
    private EditForm _form;
    
    protected override async Task OnInitializedAsync()
    {
        _timeZones = TimeZoneInfo.GetSystemTimeZones();
        await base.OnInitializedAsync();
        
        _workspace = _workspaceState.Value.List.First(
            x => x.Id == _authState.Value.Workspace!.Id
        );
        model.Fill(_workspace);
    }

    private async Task OnSave()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }
        Dispatcher.Dispatch(new UpdateWorkspaceAction(model));
    }

    private Task OnSelectedCurrency(CurrencyDto arg)
    {
        model.CurrencyId = arg.Id;
        return Task.CompletedTask;
    }
}
