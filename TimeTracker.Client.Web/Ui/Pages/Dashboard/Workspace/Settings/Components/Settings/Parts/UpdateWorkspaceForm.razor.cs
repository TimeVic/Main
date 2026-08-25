using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.List;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Workspace;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Settings.Parts;

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
        _workspace = _workspaceState.Value.List.First(
            x => x.Id == _authState.Value.Workspace!.Id
        );
        model.Fill(_workspace);
        await base.OnInitializedAsync();
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
