using System.ComponentModel;

namespace TimeTracker.Business.Common.Constants.Reports;

public enum SummaryReportType
{
    [Description("By project")]
    GroupByProject = 1,
    
    [Description("By client")]
    GroupByClient,
    
    [Description("By month")]
    GroupByMonth = 4,
    
    [Description("By week")]
    GroupByWeek = 5,
    
    [Description("By day")]
    GroupByDay = 6,
}
