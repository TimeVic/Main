using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Api.Shared.Dto.Model.Report.TeamSummary;
using TimeTracker.Client.Core.Constants;
using TimeTracker.Client.Core.Services.Security;
using TimeTracker.Client.Core.Store.Report;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Report.Summary;

public partial class SummaryReportPage
{
    private enum SummaryTab
    {
        Personal,
        Team
    }

    [Inject]
    public IState<ReportsState> _state { get; set; } = null!;

    [Inject]
    public ISecurityManager SecurityManager { get; set; } = null!;

    private SummaryTab _activeTab = SummaryTab.Personal;

    public bool _isLoaded => _state.Value.SummaryReportData != null;

    public IEnumerable<SummaryByDaysReportItemDto> _byDateItems
    {
        get => FillReportSkippedDays(_state.Value.SummaryReportData?.ByDays ?? new List<SummaryByDaysReportItemDto>());
    }

    public IEnumerable<TeamSummaryByDaysReportItemDto> _teamByDateItems
    {
        get => FillTeamReportSkippedDays(_state.Value.TeamSummaryReportData?.ByDays ?? new List<TeamSummaryByDaysReportItemDto>());
    }

    private bool IsTeamSummary => _activeTab == SummaryTab.Team;

    private bool CanViewTeamSummary => SecurityManager.HasPermission(WorkspacePermission.ReadTeamSummaryReport);

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new ReportResetSummaryReportFilterAction());
        Dispatcher.Dispatch(new ReportFetchSummaryReportAction());
    }

    private void SelectTab(SummaryTab tab)
    {
        if (_activeTab == tab || (tab == SummaryTab.Team && !CanViewTeamSummary))
        {
            return;
        }

        _activeTab = tab;
        Dispatcher.Dispatch(tab == SummaryTab.Team
            ? new ReportFetchTeamSummaryReportAction()
            : new ReportFetchSummaryReportAction());
    }

    private string GetTabClass(SummaryTab tab)
    {
        return _activeTab == tab
            ? "rounded-md bg-white px-4 py-1.5 text-sm font-medium text-slate-900 shadow"
            : "rounded-md px-4 py-1.5 text-sm font-medium text-slate-500 transition-colors hover:text-slate-900";
    }

    private ICollection<SummaryByDaysReportItemDto> FillReportSkippedDays(ICollection<SummaryByDaysReportItemDto> items)
    {
        items = items.OrderBy(item => item.Date).ToList();
        if (items.Count == 0)
        {
            return items;
        }

        if (
            _state.Value.SummaryReportFilter.PeriodType != SummaryReportPeriodType.Custom
            && _state.Value.SummaryReportFilter.PeriodType != SummaryReportPeriodType.LastMonth
            && _state.Value.SummaryReportFilter.PeriodType != SummaryReportPeriodType.ThisMonth
            && _state.Value.SummaryReportFilter.PeriodType != SummaryReportPeriodType.Past2Weeks
            && _state.Value.SummaryReportFilter.PeriodType != SummaryReportPeriodType.ThisWeek
            && _state.Value.SummaryReportFilter.PeriodType != SummaryReportPeriodType.Today
            && _state.Value.SummaryReportFilter.PeriodType != SummaryReportPeriodType.Yesterday
        )
        {
            return items;
        }

        var result = new List<SummaryByDaysReportItemDto>();
        var currentDate = items.First().Date;
        var lastDate = items.Last().Date;
        var itemsDictionary = items.ToDictionary(item => item.Date);
        while (currentDate <= lastDate)
        {
            if (itemsDictionary.TryGetValue(currentDate, out var item))
            {
                result.Add(item);
            }
            else
            {
                result.Add(new SummaryByDaysReportItemDto { Date = currentDate });
            }

            currentDate = currentDate.AddDays(1);
        }

        return result;
    }

    private static ICollection<TeamSummaryByDaysReportItemDto> FillTeamReportSkippedDays(
        ICollection<TeamSummaryByDaysReportItemDto> items
    )
    {
        var itemsByDate = items.OrderBy(item => item.Date).ToDictionary(item => item.Date);
        if (itemsByDate.Count == 0)
        {
            return items;
        }

        var result = new List<TeamSummaryByDaysReportItemDto>();
        var currentDate = itemsByDate.Keys.First();
        var lastDate = itemsByDate.Keys.Last();
        while (currentDate <= lastDate)
        {
            result.Add(itemsByDate.GetValueOrDefault(currentDate) ?? new TeamSummaryByDaysReportItemDto { Date = currentDate });
            currentDate = currentDate.AddDays(1);
        }

        return result;
    }
}
