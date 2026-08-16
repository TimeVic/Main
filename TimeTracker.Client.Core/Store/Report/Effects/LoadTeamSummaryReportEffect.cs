using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.Report.Effects;

public class LoadTeamSummaryReportEffect : Effect<ReportFetchTeamSummaryReportAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<ReportsState> _reportsState;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadTeamSummaryReportEffect> _logger;

    public LoadTeamSummaryReportEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<ReportsState> reportsState,
        ILogger<LoadTeamSummaryReportEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _reportsState = reportsState;
        _logger = logger;
    }

    public override async Task HandleAsync(ReportFetchTeamSummaryReportAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new ReportSetIsLoadingAction(true));
            var filter = _reportsState.Value.SummaryReportFilter;
            var response = await _apiService.ReportsGetTeamSummaryReportAsync(
                _authState.Value.Workspace!.Id,
                filter.StartDate,
                filter.EndDate
            );

            if (response == null)
            {
                throw new InvalidOperationException("Team summary report loading error.");
            }

            dispatcher.Dispatch(new ReportSetTeamSummaryReportAction(response));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to load the team summary report.");
        }
        finally
        {
            dispatcher.Dispatch(new ReportSetIsLoadingAction(false));
        }
    }
}
