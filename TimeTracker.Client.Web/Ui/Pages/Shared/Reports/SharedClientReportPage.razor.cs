using System.Globalization;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Model.Report.SharedClientReport;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.SharedClientReport;
using TimeTracker.Client.Core.Ui.Shared.Components.Enums;
using TimeTracker.Client.Web.Services;

namespace TimeTracker.Client.Web.Ui.Pages.Shared.Reports;

public partial class SharedClientReportPage
{
    private sealed record ProjectTasksState(
        List<SharedClientReportTaskDto> Tasks,
        int Page,
        bool IsHasMore
    );

    private sealed record BalancePresentation(
        string CardAmount,
        string BadgeText,
        string CardClass,
        string BadgeClass,
        ComponentColor BadgeColor,
        string BadgeIcon,
        string CardIcon
    );

    [Parameter]
    public string Token { get; set; } = string.Empty;

    [Inject]
    private ILocalizationUrlService LocalizationUrlService { get; set; } = null!;

    private readonly HashSet<Guid> _expandedProjectIds = [];
    private readonly Dictionary<Guid, ProjectTasksState> _tasksByProjectId = [];
    private GetSharedClientReportResponse? _report;
    private bool _isLoading = true;
    private Guid? _loadingProjectId;

    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;
        _report = null;
        _expandedProjectIds.Clear();
        _tasksByProjectId.Clear();
        _loadingProjectId = null;

        try
        {
            _report = await ApiService.ReportsGetPublicSharedClientReportAsync(Token);
            if (_report != null)
            {
                LocalizationUrlService.ApplyCulture(_report.CultureCode);
            }
        }
        catch (Exception)
        {
            _report = null;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task ToggleProjectAsync(Guid projectId)
    {
        if (_report?.IsShowTasks != true)
        {
            return;
        }

        if (!_expandedProjectIds.Add(projectId))
        {
            _expandedProjectIds.Remove(projectId);
            return;
        }

        try
        {
            if (_tasksByProjectId.ContainsKey(projectId))
            {
                return;
            }

            await LoadProjectTasksAsync(projectId, isReset: true);
        }
        catch (Exception)
        {
            _expandedProjectIds.Remove(projectId);
            ToastService.ShowError(DashboardLocalizer["SharedClientReport_TasksLoadError"].Value);
        }
    }

    private List<SharedClientReportTaskDto> GetProjectTasks(Guid projectId)
    {
        return _tasksByProjectId.TryGetValue(projectId, out var state)
            ? state.Tasks
            : [];
    }

    private bool IsTasksLoading(Guid projectId)
    {
        return _loadingProjectId == projectId;
    }

    private bool IsHasMoreTasks(Guid projectId)
    {
        return _tasksByProjectId.TryGetValue(projectId, out var state) && state.IsHasMore;
    }

    private async Task LoadMoreTasksAsync(Guid projectId)
    {
        try
        {
            await LoadProjectTasksAsync(projectId);
        }
        catch (Exception)
        {
            ToastService.ShowError(DashboardLocalizer["SharedClientReport_TasksLoadError"].Value);
        }
    }

    private async Task LoadProjectTasksAsync(Guid projectId, bool isReset = false)
    {
        _loadingProjectId = projectId;
        try
        {
            var page = 1;
            var tasks = new List<SharedClientReportTaskDto>();
            if (!isReset && _tasksByProjectId.TryGetValue(projectId, out var currentState))
            {
                page = currentState.Page + 1;
                tasks = currentState.Tasks.ToList();
            }

            var response = await ApiService.ReportsGetPublicSharedClientReportTasksAsync(Token, projectId, page);
            if (response == null)
            {
                return;
            }

            tasks.AddRange(response.Tasks);
            _tasksByProjectId[projectId] = new ProjectTasksState(tasks, page, response.IsHasMore);
        }
        finally
        {
            _loadingProjectId = null;
        }
    }

    private string FormatAmount(decimal amount)
    {
        var currencyCode = _report?.CurrencyCode;
        return string.IsNullOrWhiteSpace(currencyCode)
            ? amount.ToString("N2", CultureInfo.CurrentUICulture)
            : $"{amount.ToString("N2", CultureInfo.CurrentUICulture)} {currencyCode}";
    }

    private BalancePresentation GetBalancePresentation()
    {
        var outstanding = _report!.Totals.Outstanding;
        if (outstanding > 0)
        {
            return new BalancePresentation(
                FormatAmount(outstanding),
                DashboardLocalizer["SharedClientReport_PaymentPending"].Value,
                "border-red-200 bg-red-50/40 text-red-700",
                "bg-red-100 text-red-700",
                ComponentColor.Danger,
                "fa-clock",
                "fa-circle-exclamation"
            );
        }

        if (outstanding < 0)
        {
            return new BalancePresentation(
                FormatAmount(0),
                string.Format(DashboardLocalizer["SharedClientReport_AdvanceBalance"].Value, FormatAmount(Math.Abs(outstanding))),
                "border-emerald-200 bg-emerald-50/40 text-emerald-700",
                "bg-emerald-100 text-emerald-700",
                ComponentColor.Success,
                "fa-shield-halved",
                "fa-shield-heart"
            );
        }

        return new BalancePresentation(
            FormatAmount(0),
            DashboardLocalizer["SharedClientReport_PaidInFull"].Value,
            "border-slate-200 bg-slate-50 text-slate-700",
            "bg-slate-200 text-slate-700",
            ComponentColor.Secondary,
            "fa-circle-check",
            "fa-circle-check"
        );
    }
}
