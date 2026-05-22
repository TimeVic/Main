using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Business.Common.Constants.Notes;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Notes.Components;

public partial class NoteEditorBlock
{
    [Parameter]
    public NoteDocumentDto? Document { get; set; }

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string MarkdownContent { get; set; } = string.Empty;

    [Parameter]
    public NoteVisibility Visibility { get; set; } = NoteVisibility.Workspace;

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public bool IsSaving { get; set; }

    [Parameter]
    public bool IsDirty { get; set; }

    [Parameter]
    public string SaveStateLabel { get; set; } = string.Empty;

    [Parameter]
    public string SaveStateClass { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> TitleChanged { get; set; }

    [Parameter]
    public EventCallback<string> MarkdownContentChanged { get; set; }

    [Parameter]
    public EventCallback<NoteVisibility> VisibilityChanged { get; set; }

    [Parameter]
    public EventCallback OnSave { get; set; }

    private async Task OnTitleInput(ChangeEventArgs args)
    {
        await TitleChanged.InvokeAsync(args.Value?.ToString() ?? string.Empty);
    }

    private async Task OnMarkdownInput(ChangeEventArgs args)
    {
        await MarkdownContentChanged.InvokeAsync(args.Value?.ToString() ?? string.Empty);
    }

    private async Task OnVisibilityChanged(NoteVisibility? visibility)
    {
        await VisibilityChanged.InvokeAsync(visibility ?? NoteVisibility.Workspace);
    }
}
