using Microsoft.AspNetCore.Components;
using LumexUI;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Business.Common.Constants.Notes;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Components;

public partial class NoteEditorBlock
{
    private const int MarkdownTextareaMinRows = 24;
    private const int MarkdownTextareaMaxRows = int.MaxValue;

    private static readonly IReadOnlyDictionary<string, object> MarkdownTextareaAttributes =
        new Dictionary<string, object>
        {
            ["maxlength"] = 5000000
        };

    private static readonly InputFieldSlots MarkdownTextareaClasses = new()
    {
        InputWrapper = "min-h-[560px] border-slate-200 bg-white shadow-none transition focus-within:border-blue-500",
        InnerWrapper = "items-start",
        Input = "min-h-[560px] resize-none overflow-hidden font-mono text-sm leading-6 text-slate-800 placeholder-slate-400"
    };

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
    public bool IsEditing { get; set; }

    [Parameter]
    public bool IsEmbedded { get; set; }

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
    public EventCallback OnEdit { get; set; }

    [Parameter]
    public EventCallback OnSave { get; set; }

    private string ContainerClass => IsEmbedded
        ? "flex h-full min-h-[720px] w-full flex-col bg-white"
        : "flex min-h-[720px] flex-col rounded-2xl border border-slate-200 bg-white shadow-sm";

    private async Task OnTitleInput(ChangeEventArgs args)
    {
        await TitleChanged.InvokeAsync(args.Value?.ToString() ?? string.Empty);
    }

    private async Task OnMarkdownChanged(string value)
    {
        await MarkdownContentChanged.InvokeAsync(value);
    }

    private async Task OnVisibilityChanged(NoteVisibility? visibility)
    {
        await VisibilityChanged.InvokeAsync(visibility ?? NoteVisibility.Workspace);
    }

    private async Task OnEditClick()
    {
        await OnEdit.InvokeAsync();
    }

    private string GetVisibilityLabel()
    {
        var key = $"{nameof(NoteVisibility)}_{Visibility}";
        var localized = DashboardLocalizer[key];
        return localized.ResourceNotFound ? Visibility.ToString() : localized.Value;
    }
}
