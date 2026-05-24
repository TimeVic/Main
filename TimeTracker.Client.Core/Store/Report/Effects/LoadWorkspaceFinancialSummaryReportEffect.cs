using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.Report.Effects;

public class LoadWorkspaceFinancialSummaryReportEffect : Effect<ReportFetchWorkspaceFinancialSummaryAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<ReportsState> _reportsState;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadWorkspaceFinancialSummaryReportEffect> _logger;

    public LoadWorkspaceFinancialSummaryReportEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<ReportsState> reportsState,
        ILogger<LoadWorkspaceFinancialSummaryReportEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _reportsState = reportsState;
        _logger = logger;
    }

    public override async Task HandleAsync(ReportFetchWorkspaceFinancialSummaryAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new ReportSetIsLoadingAction(true));
            var response = await _apiService.ReportsGetWorkspaceFinancialSummaryAsync(_authState.Value.Workspace!.Id);
            if (response == null)
                throw new Exception("Workspace financial summary report loading error");
            dispatcher.Dispatch(new ReportSetWorkspaceFinancialSummaryAction(response));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new ReportSetIsLoadingAction(false));
        }
    }
}
