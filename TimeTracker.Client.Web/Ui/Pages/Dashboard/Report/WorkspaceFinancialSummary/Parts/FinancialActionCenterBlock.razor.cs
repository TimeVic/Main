using Microsoft.AspNetCore.Components;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Report.WorkspaceFinancialSummary.Parts;

public partial class FinancialActionCenterBlock
{
    [Parameter]
    public EventCallback<Guid> OnPayMember { get; set; }

    [Parameter]
    public EventCallback<(Guid ClientId, string ClientName)> OnShareClient { get; set; }

    [Inject]
    public UiHelperService UiHelperService { get; set; } = null!;

    private bool IsCanShareClientReport => AuthState.Value.Workspace?.IsWorkspaceAdmin == true;

    private async Task CopyReminderAsync(string clientName, decimal outstanding)
    {
        var reminder = string.Format(
            DashboardLocalizer["WorkspaceMoney_ReminderTemplate"].Value,
            clientName,
            outstanding.ToString("0.##")
        );

        if (await UiHelperService.CopyToClipboard(reminder))
        {
            ToastService.ShowSuccess(DashboardLocalizer["WorkspaceMoney_ReminderCopied"].Value);
        }
    }
}
