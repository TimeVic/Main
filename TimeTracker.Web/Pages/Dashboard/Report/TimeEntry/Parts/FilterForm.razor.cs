using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Pages.Dashboard.Report.TimeEntry.Parts;

public partial class FilterForm
{
    [Inject]
    public IState<TimeEntryState> _state { get; set; }

    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }

    private void OnChangeClient(long clientId)
    {
        _state.Value.Filter.ClientId = clientId == 0 ? null : clientId;
        UpdateFilterState();
    }

    private void OnChangeProject(long projectId)
    {
        _state.Value.Filter.ProjectId = projectId == 0 ? null : projectId;
        UpdateFilterState();
    }

    private void OnChangeSearch(string search)
    {
        _state.Value.Filter.Search = search ?? "";
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
