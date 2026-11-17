using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TimeTracker.Api.Shared.Dto.Entity.Notes;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Components;

public partial class NotesDraftsBlock
{
    [Parameter]
    public IReadOnlyList<NoteTreeNodeDto> Drafts { get; set; } = Array.Empty<NoteTreeNodeDto>();

    [Parameter]
    public Guid? SelectedNoteId { get; set; }

    [Parameter]
    public bool IsCreatingDraft { get; set; }

    [Parameter]
    public bool IsDeletingDraft { get; set; }

    [Parameter]
    public EventCallback CreateDraftRequested { get; set; }

    [Parameter]
    public EventCallback<NoteTreeNodeDto> SelectDraftRequested { get; set; }

    [Parameter]
    public EventCallback<Guid> DeleteDraftRequested { get; set; }

    private bool IsSelected(Guid draftId) => SelectedNoteId == draftId;

    private string GetDraftRowClass(Guid draftId) => IsSelected(draftId)
        ? "group flex cursor-pointer items-center justify-between gap-2 rounded-lg border border-blue-200 bg-blue-50 px-2 py-1.5 font-medium text-blue-900 outline-none focus-visible:ring-2 focus-visible:ring-blue-200"
        : "group flex cursor-pointer items-center justify-between gap-2 rounded-lg border border-transparent px-2 py-1.5 text-slate-700 outline-none hover:border-slate-200 hover:bg-white focus-visible:ring-2 focus-visible:ring-blue-200";

    private async Task OnCreateDraftClick()
    {
        await CreateDraftRequested.InvokeAsync();
    }

    private async Task OnDraftKeyDown(KeyboardEventArgs args, NoteTreeNodeDto draft)
    {
        if (args.Key is not ("Enter" or " " or "Spacebar"))
        {
            return;
        }

        await SelectDraftRequested.InvokeAsync(draft);
    }

    private string FormatDraftTime(DateTime? updatedAtUtc)
    {
        if (!updatedAtUtc.HasValue)
        {
            return string.Empty;
        }

        var local = updatedAtUtc.Value.ToLocalTime();
        var elapsed = DateTime.UtcNow - updatedAtUtc.Value;

        if (elapsed.TotalMinutes < 1)
        {
            return DashboardLocalizer["Notes_DraftTimeJustNow"].Value;
        }

        if (elapsed.TotalHours < 1)
        {
            return string.Format(
                DashboardLocalizer["Notes_DraftTimeMinutesAgo"].Value,
                (int)elapsed.TotalMinutes
            );
        }

        if (local.Date == DateTime.Now.Date)
        {
            return local.ToString("HH:mm");
        }

        return local.ToString("MMM d");
    }
}
