using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

namespace TimeTracker.Client.Core.Services.Http;

public partial class ApiService
{
    public async Task<GetNotesTreeResponse?> NotesGetTreeAsync(GetNotesTreeRequest model)
    {
        return await PostAsync<GetNotesTreeResponse?>(ApiUrl.NotesGetTree, model);
    }

    public async Task<NoteDocumentDto?> NotesGetDocumentAsync(GetNoteDocumentRequest model)
    {
        return await PostAsync<NoteDocumentDto?>(ApiUrl.NotesGetDocument, model);
    }

    public async Task<NoteTreeNodeDto?> NotesCreateFolderAsync(CreateNoteFolderRequest model)
    {
        return await PostAsync<NoteTreeNodeDto?>(ApiUrl.NotesCreateFolder, model);
    }

    public async Task<NoteDocumentDto?> NotesCreateDocumentAsync(CreateNoteDocumentRequest model)
    {
        return await PostAsync<NoteDocumentDto?>(ApiUrl.NotesCreateDocument, model);
    }

    public async Task<NoteDocumentDto?> NotesUpdateDocumentAsync(UpdateNoteDocumentRequest model)
    {
        return await PostAsync<NoteDocumentDto?>(ApiUrl.NotesUpdateDocument, model);
    }

    public async Task<NoteTreeNodeDto?> NotesRenameNodeAsync(RenameNoteNodeRequest model)
    {
        return await PostAsync<NoteTreeNodeDto?>(ApiUrl.NotesRenameNode, model);
    }

    public async Task<NoteTreeNodeDto?> NotesMoveNodeAsync(MoveNoteNodeRequest model)
    {
        return await PostAsync<NoteTreeNodeDto?>(ApiUrl.NotesMoveNode, model);
    }

    public async Task<NoteTreeNodeDto?> NotesArchiveNodeAsync(ArchiveNoteNodeRequest model)
    {
        return await PostAsync<NoteTreeNodeDto?>(ApiUrl.NotesArchiveNode, model);
    }

    public async Task<GetLinkedNotesResponse?> NotesGetLinkedNotesAsync(GetLinkedNotesRequest model)
    {
        return await PostAsync<GetLinkedNotesResponse?>(ApiUrl.NotesGetLinkedNotes, model);
    }

    public async Task<NoteLinkDto?> NotesCreateLinkAsync(CreateNoteLinkRequest model)
    {
        return await PostAsync<NoteLinkDto?>(ApiUrl.NotesCreateLink, model);
    }

    public async Task NotesDeleteLinkAsync(DeleteNoteLinkRequest model)
    {
        await PostAsync<object?>(ApiUrl.NotesDeleteLink, model);
    }
}
