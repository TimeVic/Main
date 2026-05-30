using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Extensions;
using TimeTracker.Client.Core.Constants;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.TimeEntry;

namespace TimeTracker.Client.Core.Store.Report.Effects;

public class LoadSummaryReportEffect: Effect<ReportFetchSummaryReportAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<ReportsState> _reportsState;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadMemberPaymentReportEffect> _logger;

    public LoadSummaryReportEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<ReportsState> reportsState,
        ILogger<LoadMemberPaymentReportEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _reportsState = reportsState;
        _logger = logger;
    }

    public override async Task HandleAsync(ReportFetchSummaryReportAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new ReportSetIsLoadingAction(true));
            var response = await _apiService.ReportsGetSummaryReportAsync(
                _authState.Value.Workspace!.Id,
                _reportsState.Value.SummaryReportFilter.StartDate,
                _reportsState.Value.SummaryReportFilter.EndDate,
                _reportsState.Value.SummaryReportFilter.ReportType
            );
            dispatcher.Dispatch(new ReportSetSummaryReportItemsAction(response));
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
