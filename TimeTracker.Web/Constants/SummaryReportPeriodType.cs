using System.ComponentModel;

namespace TimeTracker.Web.Constants;

public enum SummaryReportPeriodType
{
    [Description("Today")]
    Today = 1,
    
    [Description("Yesterday")]
    Yesterday,
    
    [Description("This week")]
    ThisWeek,
    
    [Description("Last week")]
    LastWeek,
    
    [Description("Past 2 weeks")]
    Past2Weeks,
    
    [Description("This month")]
    ThisMonth,
    
    [Description("Last month")]
    LastMonth,
    
    [Description("This year")]
    ThisYear,
    
    [Description("Last year")]
    LastYear,
    
    [Description("Custom")]
    Custom
}
