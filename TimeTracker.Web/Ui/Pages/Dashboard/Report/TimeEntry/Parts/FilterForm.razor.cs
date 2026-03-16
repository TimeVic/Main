using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Report.TimeEntry.Parts;

public partial class FilterForm
{
    [Inject]
    public IState<TimeEntryState> _state { get; set; }

    private void OnChangeClient(ClientDto? client)
    {
        _state.Value.Filter.ClientId = client?.Id;
        UpdateFilterState();
    }

    private void OnChangeProject(ProjectDto? project)
    {
        _state.Value.Filter.ProjectId = project?.Id;
        UpdateFilterState();
    }

    private void OnChangeSearch(string? search)
    {
        _state.Value.Filter.Search = search;
        UpdateFilterState();
    }
    
    private void OnChangeDateFrom(DateTime? from)
    {
        _state.Value.Filter.DateFrom = from?.StartOfDay();
        UpdateFilterState();
    }
    
    private void OnChangeDateTo(DateTime? to)
    {
        _state.Value.Filter.DateTo = to?.EndOfDay();
        UpdateFilterState();
    }

    private void OnChangeMember(WorkspaceMembershipDto? member)
    {
        _state.Value.Filter.UserId = member?.User.Id;
        UpdateFilterState();
    }
    
    private void UpdateFilterState()
    {
        Dispatcher.Dispatch(new SetTimeEntryFilterAction(_state.Value.Filter));
        Dispatcher.Dispatch(new LoadTimeEntryFilteredListAction());
    }
}
