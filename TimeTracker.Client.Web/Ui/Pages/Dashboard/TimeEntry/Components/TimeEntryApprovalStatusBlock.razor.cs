using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.TimeEntry.Components;

public partial class TimeEntryApprovalStatusBlock
{
    [Parameter]
    public TimeEntryDto? Entry { get; set; }

    [Parameter]
    public bool IsApprovalsEnabled { get; set; }
}
