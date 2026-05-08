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
    private string? _search;
    private Guid SelectedClientId { get; set; }
    private Guid SelectedProjectId { get; set; }

    private bool CanCreatePayments => SecurityManager.HasPermission(WorkspacePermission.CreateClientPayment);

    private IReadOnlyCollection<ClientPaymentDto> FilteredPayments => _state.Value.List
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

    protected override void OnInitialized()
    {
        base.OnInitialized();
        WorkspacePermissionsState.StateChanged += OnWorkspacePermissionsStateChanged;
        _dispatcher.Dispatch(new LoadClientPaymentListAction(true));
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

    private void OnWorkspacePermissionsStateChanged(object? sender, EventArgs args)
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        WorkspacePermissionsState.StateChanged -= OnWorkspacePermissionsStateChanged;
    }
}
