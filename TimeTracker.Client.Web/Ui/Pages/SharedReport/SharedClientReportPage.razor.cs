using System.Globalization;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Model.Report.SharedClientReport;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.SharedClientReport;

namespace TimeTracker.Client.Web.Ui.Pages.SharedReport;

public partial class SharedClientReportPage
{
    private const int TasksPreviewLimit = 10;

    [Parameter]
    public string Token { get; set; } = string.Empty;

    private readonly HashSet<Guid> _expandedProjectIds = [];
    private readonly HashSet<Guid> _fullyExpandedTaskProjectIds = [];
    private readonly Dictionary<Guid, List<SharedClientReportTaskDto>> _tasksByProjectId = [];
    private GetSharedClientReportResponse? _report;
    private bool _isLoading = true;
    private bool _isTasksLoading;
    private bool _isTasksLoaded;

    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;
        _report = null;
        _expandedProjectIds.Clear();
        _fullyExpandedTaskProjectIds.Clear();
        _tasksByProjectId.Clear();
        _isTasksLoaded = false;

        try
        {
            _report = await ApiService.ReportsGetPublicSharedClientReportAsync(Token);
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

        if (_isTasksLoaded)
        {
            return;
        }

        _isTasksLoading = true;
        try
        {
            var response = await ApiService.ReportsGetPublicSharedClientReportTasksAsync(Token);
            foreach (var task in response?.Tasks ?? [])
            {
                if (!_tasksByProjectId.TryGetValue(task.ProjectId, out var tasks))
                {
                    tasks = [];
                    _tasksByProjectId[task.ProjectId] = tasks;
                }

                tasks.Add(task);
            }

            foreach (var tasks in _tasksByProjectId.Values)
            {
                tasks.Sort((left, right) => right.Duration.CompareTo(left.Duration));
            }

            _isTasksLoaded = true;
        }
        catch (Exception)
        {
            _expandedProjectIds.Remove(projectId);
            ToastService.ShowError(DashboardLocalizer["SharedClientReport_TasksLoadError"].Value);
        }
        finally
        {
            _isTasksLoading = false;
        }
    }

    private List<SharedClientReportTaskDto> GetProjectTasks(Guid projectId)
    {
        return _tasksByProjectId.GetValueOrDefault(projectId, []);
    }

    private IEnumerable<SharedClientReportTaskDto> GetVisibleTasks(Guid projectId)
    {
        var tasks = GetProjectTasks(projectId);
        return _fullyExpandedTaskProjectIds.Contains(projectId)
            ? tasks
            : tasks.Take(TasksPreviewLimit);
    }

    private string FormatAmount(decimal amount)
    {
        var currencyCode = _report?.CurrencyCode;
        return string.IsNullOrWhiteSpace(currencyCode)
            ? amount.ToString("N2", CultureInfo.CurrentUICulture)
            : $"{amount.ToString("N2", CultureInfo.CurrentUICulture)} {currencyCode}";
    }

    private string GetBalanceCardClass()
    {
        return _report!.Totals.Outstanding > 0
            ? "border-red-200 bg-red-50/40 text-red-700"
            : "border-emerald-200 bg-emerald-50/40 text-emerald-700";
    }

    private string GetBalanceBadgeClass()
    {
        return _report!.Totals.Outstanding > 0
            ? "bg-red-100 text-red-700"
            : "bg-emerald-100 text-emerald-700";
    }

    private string GetBalanceText()
    {
        return _report!.Totals.Outstanding > 0
            ? DashboardLocalizer["SharedClientReport_PaymentPending"].Value
            : DashboardLocalizer["SharedClientReport_Advance"].Value;
    }
}
