using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Client.Core.Constants;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Components;

public partial class ProjectNotesSectionBlock
{
    [Parameter]
    public required ProjectDto Project { get; set; }

    [Inject]
    private ILogger<ProjectNotesSectionBlock> Logger { get; set; } = null!;

    private IReadOnlyList<NoteTreeNodeDto> _notes = [];
    private Guid? _loadedProjectId;
    private bool _isLoading;
    private bool _isCreatingNote;
    private string? _loadError;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        if (_loadedProjectId == Project.Id)
        {
            return;
        }

        _loadedProjectId = Project.Id;
        await LoadLinkedNotes();
    }

    private async Task LoadLinkedNotes()
    {
        _isLoading = true;
        _loadError = null;
        try
        {
            var response = await ApiService.NotesGetLinkedNotesAsync(new GetLinkedNotesRequest
            {
                EntityType = NoteLinkEntityType.Project,
                EntityId = Project.Id
            });
            _notes = response?.Notes
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
                .ToList() ?? [];
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to load notes linked to project {ProjectId}", Project.Id);
            _loadError = DashboardLocalizer["Notes_LoadLinkedNotesError"].Value;
            ToastService.ShowError(DashboardLocalizer["Notes_LoadLinkedNotesError"].Value);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task CreateProjectNote()
    {
        _isCreatingNote = true;
        try
        {
            var document = await ApiService.NotesCreateDocumentAsync(new CreateNoteDocumentRequest
            {
                Title = string.Format(DashboardLocalizer["Notes_ProjectNoteDefaultTitle"].Value, Project.Name),
                MarkdownContent = string.Empty,
                Visibility = NoteVisibility.Workspace,
                Links =
                [
                    new NoteLinkRequestDto
                    {
                        EntityType = NoteLinkEntityType.Project,
                        EntityId = Project.Id
                    }
                ]
            });
            if (document == null)
            {
                throw new InvalidOperationException("Project note create response is empty.");
            }

            NavigateToNote(document.Id);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to create note linked to project {ProjectId}", Project.Id);
            ToastService.ShowError(DashboardLocalizer["Notes_CreateProjectNoteError"].Value);
        }
        finally
        {
            _isCreatingNote = false;
        }
    }

    private void NavigateToNote(Guid noteId)
    {
        NavigationManager.NavigateTo($"{SiteUrl.Dashboard_Notes}?noteId={noteId}");
    }
}
