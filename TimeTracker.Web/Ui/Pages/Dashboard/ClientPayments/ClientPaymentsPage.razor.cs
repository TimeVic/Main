using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Services.Security;
using TimeTracker.Web.Store.ClientPayments;
using TimeTracker.Web.Store.Permissions;

namespace TimeTracker.Web.Ui.Pages.Dashboard.ClientPayments;

public partial class ClientPaymentsPage
{
    [Inject]
    public IState<ClientPaymentState> _state { get; set; }

    [Inject]
    public IState<WorkspacePermissionsState> WorkspacePermissionsState { get; set; }

    [Inject]
    public ISecurityManager SecurityManager { get; set; }

    [Inject]
    public IDispatcher _dispatcher { get; set; }

    private bool _isShowAddClientPaymentModal;
    private ClientPaymentPeriodFilter _periodFilter = ClientPaymentPeriodFilter.ThisMonth;
    private string? _search;
    private Guid SelectedClientId { get; set; }
    private Guid SelectedProjectId { get; set; }
    private DateTime? _customFrom;
    private DateTime? _customTo;

    private bool CanCreatePayments => SecurityManager.HasPermission(WorkspacePermission.CreateClientPayment);

    private IReadOnlyCollection<ClientPaymentDto> FilteredPayments => _state.Value.List
        .Where(MatchesPeriod)
        .Where(MatchesClient)
        .Where(MatchesProject)
        .Where(MatchesSearch)
        .OrderByDescending(item => item.PaymentTime)
        .ToList();

    private decimal FilteredTotal => FilteredPayments.Sum(item => item.Amount);

    private int PaidClientsCount => FilteredPayments
        .Select(item => item.Client.Id)
        .Distinct()
        .Count();

    private string CustomFromText => _customFrom?.ToString("yyyy-MM-dd") ?? string.Empty;

    private string CustomToText => _customTo?.ToString("yyyy-MM-dd") ?? string.Empty;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        WorkspacePermissionsState.StateChanged += OnWorkspacePermissionsStateChanged;
        _dispatcher.Dispatch(new LoadClientPaymentListAction(true));
    }

    private bool MatchesPeriod(ClientPaymentDto payment)
    {
        var paymentDate = payment.PaymentTime.Date;
        var today = DateTime.Today;
        var thisMonthStart = new DateTime(today.Year, today.Month, 1);

        return _periodFilter switch
        {
            ClientPaymentPeriodFilter.ThisMonth => paymentDate >= thisMonthStart && paymentDate < thisMonthStart.AddMonths(1),
            ClientPaymentPeriodFilter.LastMonth => paymentDate >= thisMonthStart.AddMonths(-1) && paymentDate < thisMonthStart,
            ClientPaymentPeriodFilter.Custom => (!_customFrom.HasValue || paymentDate >= _customFrom.Value.Date)
                && (!_customTo.HasValue || paymentDate <= _customTo.Value.Date),
            _ => true
        };
    }

    private bool MatchesClient(ClientPaymentDto payment)
    {
        return SelectedClientId == Guid.Empty || payment.Client.Id == SelectedClientId;
    }

    private bool MatchesProject(ClientPaymentDto payment)
    {
        return SelectedProjectId == Guid.Empty || payment.Project?.Id == SelectedProjectId;
    }

    private bool MatchesSearch(ClientPaymentDto payment)
    {
        if (string.IsNullOrWhiteSpace(_search))
        {
            return true;
        }

        var search = _search.Trim();
        return Contains(payment.Client.Name, search)
            || Contains(payment.Project?.Name, search)
            || Contains(payment.Description, search)
            || Contains(payment.Id.ToString(), search);
    }

    private static bool Contains(string? value, string search)
    {
        return value?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    private void OnPeriodChanged(ChangeEventArgs args)
    {
        if (Enum.TryParse<ClientPaymentPeriodFilter>(args.Value?.ToString(), out var value))
        {
            _periodFilter = value;
        }
    }

    private void OnSearchChanged(ChangeEventArgs args)
    {
        _search = args.Value?.ToString();
    }

    private void OnClientFilterChanged(ClientDto? client)
    {
        SelectedClientId = client?.Id ?? Guid.Empty;
        SelectedProjectId = Guid.Empty;
    }

    private void OnProjectFilterChanged(ProjectDto? project)
    {
        SelectedProjectId = project?.Id ?? Guid.Empty;
    }

    private void OnCustomFromChanged(ChangeEventArgs args)
    {
        _customFrom = ParseDate(args.Value?.ToString());
    }

    private void OnCustomToChanged(ChangeEventArgs args)
    {
        _customTo = ParseDate(args.Value?.ToString());
    }

    private static DateTime? ParseDate(string? value)
    {
        return DateTime.TryParse(value, out var date) ? date : null;
    }

    private void OnWorkspacePermissionsStateChanged(object? sender, EventArgs args)
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        WorkspacePermissionsState.StateChanged -= OnWorkspacePermissionsStateChanged;
    }
}

public enum ClientPaymentPeriodFilter
{
    ThisMonth,
    LastMonth,
    AllTime,
    Custom
}
