using Fluxor;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Store.Report;

public class ReportReducers
{
    #region MemberPayment
    
    [ReducerMethod]
    public static ReportsState Reducer(ReportsState state, ReportSetMemberPaymentReportFilterAction action)
    {
        return state with
        {
            MemberPaymentReportFilter = action.FilterState
        };
    }
    
    [ReducerMethod]
    public static ReportsState Reducer(ReportsState state, ReportSetMemberPaymentReportItemsAction action)
    {
        return state with
        {
            MemberPaymentReportItems = action.Items
        };
    }
    
    [ReducerMethod]
    public static ReportsState Reducer(ReportsState state, ReportSetIsLoadingAction action)
    {
        return state with
        {
            IsLoading = action.IsLoading
        };
    }
    
    #endregion
    
    #region Summary
    
    [ReducerMethod]
    public static ReportsState Reducer(ReportsState state, ReportSetSummaryReportItemsAction action)
    {
        return state with
        {
            SummaryReportData = action.ReportData
        };
    }
    
    [ReducerMethod]
    public static ReportsState Reducer(ReportsState state, ReportSetSummaryReportFilterAction action)
    {
        var startDate = action.FilterState.StartDate;
        var endDate = action.FilterState.EndDate;
        var periodType = action.FilterState.PeriodType;
        if (periodType != SummaryReportPeriodType.Custom)
        {
            (startDate, endDate) = GetPeriodBasedOnPeriodType(periodType);
        }

        action.FilterState = action.FilterState with
        {
            StartDate = startDate,
            EndDate = endDate
        };
        
        return state with
        {
            SummaryReportFilter = action.FilterState
        };
    }
    
    [ReducerMethod]
    public static ReportsState Reducer(ReportsState state, ReportResetSummaryReportFilterAction action)
    {
        return state with
        {
            SummaryReportFilter = state.SummaryReportFilter with
            {
                ReportType = SummaryReportType.GroupByDay,
                PeriodType = SummaryReportPeriodType.Today,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now
            }
        };
    }
    
    private static (DateTime startTime, DateTime endTime) GetPeriodBasedOnPeriodType(SummaryReportPeriodType periodType)
    {
        var startTime = DateTime.UtcNow;
        var endTime = DateTime.UtcNow;
        if (periodType == SummaryReportPeriodType.Today)
        {
            startTime = DateTime.UtcNow;
        }
        else if (periodType == SummaryReportPeriodType.Yesterday)
        {
            endTime = startTime = DateTime.UtcNow.AddDays(-1);
        }
        else if (periodType == SummaryReportPeriodType.ThisWeek)
        {
            startTime = DateTime.UtcNow.StartOfWeek();
        }
        else if (periodType == SummaryReportPeriodType.ThisMonth)
        {
            startTime = DateTime.UtcNow.StartOfMonth();
        }
        else if (periodType == SummaryReportPeriodType.ThisYear)
        {
            startTime = endTime.StartOfYear();
            endTime = DateTime.UtcNow.EndOfYear();
        }
        else if (periodType == SummaryReportPeriodType.LastWeek)
        {
            endTime = DateTime.UtcNow.StartOfWeek().AddDays(-1);
            startTime = endTime.AddDays(-6);
        }
        else if (periodType == SummaryReportPeriodType.Past2Weeks)
        {
            endTime = DateTime.UtcNow;
            startTime = endTime.StartOfWeek().AddDays(-1).StartOfWeek();
        }
        else if (periodType == SummaryReportPeriodType.LastMonth)
        {
            endTime = DateTime.UtcNow.StartOfMonth().AddDays(-1);
            startTime = endTime.StartOfMonth();
        }
        else if (periodType == SummaryReportPeriodType.LastYear)
        {
            endTime = DateTime.UtcNow.StartOfYear().AddDays(-1);
            startTime = endTime.StartOfYear();
        }
        
        return (startTime, endTime);
    }
    
    #endregion
}
