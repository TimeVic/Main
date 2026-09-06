using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Business.Extensions;
using TimeTracker.Client.Core.Store.TimeEntry;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Report.TimeEntry.Parts;

public partial class TimeEntryListBlock
{
    [Inject]
    private IState<TimeEntryState> _state { get; set; } = null!;

    [Inject]
    private ITimeParsingService _timeParsingService { get; set; } = null!;

    [Inject]
    private ISecurityManager SecurityManager { get; set; } = null!;

    [Inject]
    private TimeTracker.Client.Web.Services.UI.IModalDialogProviderService _modalDialogService { get; set; } = null!;

    private async Task OnEdit(TimeEntryDto entry)
    {
        await _modalDialogService.ShowEditTimeEntryModal(entry, onClose: _ =>
        {
            Dispatcher.Dispatch(new LoadTimeEntryFilteredListAction());
        });
    }

    private async Task OnDelete(TimeEntryDto entry)
    {
        var confirmed = await _modalDialogService.ShowConfirmationAsync(
            DashboardLocalizer["DeleteTimeEntry"].Value,
            DashboardLocalizer["AreYouSure"].Value
        );
        if (confirmed)
        {
            Dispatcher.Dispatch(new DeleteTimeEntryAction(entry.Id));
            Dispatcher.Dispatch(new LoadTimeEntryFilteredListAction());
        }
    }

    private void OnPageChanged(int page)
    {
        Dispatcher.Dispatch(new SetFilteredSelectedPageAction(page));
        Dispatcher.Dispatch(new LoadTimeEntryFilteredListAction());
    }

    private static string GetDescriptionOrTask(TimeEntryDto entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.Description))
            return entry.Description.TruncateAndAddDots(120);

        if (!string.IsNullOrWhiteSpace(entry.Task?.Title))
            return entry.Task.Title.TruncateAndAddDots(120);

        return string.Empty;
    }
}
