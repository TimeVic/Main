using System.ComponentModel;

namespace TimeTracker.Business.Common.Constants.Reports;

public enum SummaryReportType
{
    [Description("By project")]
    GroupByProject = 1,
    
    [Description("By client")]
    GroupByClient,
    
    [Description("By user")]
    GroupByUser,
    
    [Description("By month")]
    GroupByMonth,
    
    [Description("By week")]
    GroupByWeek,
    
    [Description("By day")]
    GroupByDay,
}
