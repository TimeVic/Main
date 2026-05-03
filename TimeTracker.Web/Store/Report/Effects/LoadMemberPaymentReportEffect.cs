using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Store.Report.Effects;

public class LoadMemberPaymentReportEffect: Effect<ReportFetchMemberPaymentsReportAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<ReportsState> _reportState;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadMemberPaymentReportEffect> _logger;

    public LoadMemberPaymentReportEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<ReportsState> reportState,
        ILogger<LoadMemberPaymentReportEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _reportState = reportState;
        _logger = logger;
    }

    public override async Task HandleAsync(ReportFetchMemberPaymentsReportAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new ReportSetIsLoadingAction(true));
            var response = await _apiService.ReportsGetMemberPaymentsReportAsync(
                _authState.Value.Workspace!.Id,
                _reportState.Value.MemberPaymentReportFilter.EndDate
            );
            if (response == null)
                throw new Exception("Report loading error");
            dispatcher.Dispatch(new ReportSetMemberPaymentReportItemsAction(response.Items));
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
