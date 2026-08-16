using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.Report.Effects;

public class LoadUserPaymentReportEffect : Effect<ReportFetchUserPaymentReportAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<ReportsState> _reportsState;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadUserPaymentReportEffect> _logger;

    public LoadUserPaymentReportEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<ReportsState> reportsState,
        ILogger<LoadUserPaymentReportEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _reportsState = reportsState;
        _logger = logger;
    }

    public override async Task HandleAsync(ReportFetchUserPaymentReportAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new ReportSetIsLoadingAction(true));
            var response = await _apiService.ReportsGetUserPaymentReportAsync(
                _authState.Value.Workspace!.Id,
                _reportsState.Value.UserPaymentReportFilter.EndDate
            );
            if (response == null)
            {
                throw new Exception("User payment report loading error");
            }

            dispatcher.Dispatch(new ReportSetUserPaymentReportAction(response));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to load the user payment report");
        }
        finally
        {
            dispatcher.Dispatch(new ReportSetIsLoadingAction(false));
        }
    }
}
