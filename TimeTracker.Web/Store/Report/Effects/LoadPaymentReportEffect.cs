using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Store.Report.Effects;

public class LoadPaymentReportEffect: Effect<ReportFetchPaymentsReportAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<ReportsState> _reportState;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadPaymentReportEffect> _logger;

    public LoadPaymentReportEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<ReportsState> reportState,
        ILogger<LoadPaymentReportEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _reportState = reportState;
        _logger = logger;
    }

    public override async Task HandleAsync(ReportFetchPaymentsReportAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new ReportSetIsLoadingAction(true));
            var response = await _apiService.ReportsGetPaymentsReportAsync(
                _authState.Value.Workspace.Id,
                _reportState.Value.PaymentReportFilter.EndDate
            );
            dispatcher.Dispatch(new ReportSetPaymentReportItemsAction(response.Items));
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
