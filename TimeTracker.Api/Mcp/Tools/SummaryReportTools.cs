using System.ComponentModel;
using ModelContextProtocol.Server;
using TimeTracker.Api.Mcp.Context;
using TimeTracker.Api.Services.Report;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Mcp.Tools;

[McpServerToolType]
public sealed class SummaryReportTools
{
    private readonly IMcpContextService _mcpContext;
    private readonly ISecurityManager _securityManager;
    private readonly ISummaryReportService _summaryReportService;

    public SummaryReportTools(
        IMcpContextService mcpContext,
        ISecurityManager securityManager,
        ISummaryReportService summaryReportService
    )
    {
        _mcpContext = mcpContext;
        _securityManager = securityManager;
        _summaryReportService = summaryReportService;
    }

    [McpServerTool(Name = "get_summary_report", ReadOnly = true)]
    [Description("Retrieves a summary report of logged time for the specified workspace and date range, optionally grouped by day, project, client, month, or week.")]
    public async Task<SummaryReportResponse> GetSummaryReport(
        [Description("Workspace ID to generate report for")] Guid workspaceId,
        [Description("Start date and time for the report period in UTC")] DateTime startTime,
        [Description("End date and time for the report period in UTC")] DateTime endTime,
        [Description("Grouping type: GroupByProject, GroupByClient, GroupByMonth, GroupByWeek, or GroupByDay (default: GroupByDay)")] SummaryReportType type = SummaryReportType.GroupByDay
    )
    {
        var (user, workspace) = await _mcpContext.GetUserAndWorkspaceAsync(workspaceId);
        if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
        {
            throw new HasNoAccessException();
        }

        return await _summaryReportService.GetReportAsync(
            user,
            workspace.Id,
            startTime,
            endTime,
            type
        );
    }
}

