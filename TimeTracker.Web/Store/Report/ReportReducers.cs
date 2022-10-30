using Fluxor;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Store.Report;

public class ReportReducers
{
    [ReducerMethod]
    public static ReportsState ReportSetPaymentReportItemsActionReducer(ReportsState state, ReportSetPaymentReportItemsAction action)
    {
        return state with
        {
            PaymentReportItems = action.Items
        };
    }
    
    [ReducerMethod]
    public static ReportsState ReportSetPaymentReportItemsActionReducer(ReportsState state, ReportSetIsLoadingAction action)
    {
        return state with
        {
            IsLoading = action.IsLoading
        };
    }
    
    #region Summary
    
    [ReducerMethod]
    public static ReportsState ReportSetSummaryReportItemsActionReducer(ReportsState state, ReportSetSummaryReportItemsAction action)
    {
        return state with
        {
            SummaryReportData = action.ReportData
        };
    }
    
    #endregion
}
