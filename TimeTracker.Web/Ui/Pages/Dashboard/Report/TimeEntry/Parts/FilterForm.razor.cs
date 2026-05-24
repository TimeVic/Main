using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Extensions;
using TimeTracker.Client.Core.Store.TimeEntry;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Report.TimeEntry.Parts;

public partial class FilterForm
{
    [Inject]
    public IState<TimeEntryState> _state { get; set; }

    private void OnChangeProject(ProjectDto? project)
    {
        UpdateFilter(_state.Value.Filter with { ClientId = project?.Client?.Id, ProjectId = project?.Id });
    }

    private void OnChangeSearch(string? search)
    {
        UpdateFilter(_state.Value.Filter with { Search = search });
    }

    private void OnChangeDateFrom(DateTime? from)
    {
        UpdateFilter(_state.Value.Filter with { DateFrom = from?.StartOfDay() });
    }

    private void OnChangeDateTo(DateTime? to)
    {
        UpdateFilter(_state.Value.Filter with { DateTo = to?.EndOfDay() });
    }

    private void OnChangeMember(WorkspaceMemberDto? member)
    {
        UpdateFilter(_state.Value.Filter with { UserId = member?.User.Id });
    }

    private void OnChangeBillable(bool? isBillable)
    {
        UpdateFilter(_state.Value.Filter with { IsBillable = isBillable });
    }

    private void UpdateFilter(TimeEntryFilterState newFilter)
    {
        Dispatcher.Dispatch(new SetTimeEntryFilterAction(newFilter));
        Dispatcher.Dispatch(new SetFilteredSelectedPageAction(1));
        Dispatcher.Dispatch(new LoadTimeEntryFilteredListAction());
    }
}
