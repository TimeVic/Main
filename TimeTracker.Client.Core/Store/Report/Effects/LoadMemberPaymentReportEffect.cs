using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.TimeEntry;

namespace TimeTracker.Client.Core.Store.Report.Effects;

public class LoadMemberPaymentReportEffect: Effect<ReportFetchMemberPaymentsReportAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<ReportsState> _reportState;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadMemberPaymentReportEffect> _logger;

    public LoadMemberPaymentReportEffect(
        IApiService apiService,
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
