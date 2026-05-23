using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Mvc.Controllers;

namespace TimeTracker.Api.Controllers.Dashboard.Notes;

[ApiController]
[Authorize]
[Route("/dashboard/notes")]
public class NotesController(ILifetimeScope scope) : MainApiControllerBase(scope)
{
    [HttpPost("get-tree")]
    public Task<IActionResult> GetTree([FromBody] GetNotesTreeRequest request)
        => this.RequestAsync()
            .For<GetNotesTreeResponse>()
            .With(request);

    [HttpPost("get-document")]
    public Task<IActionResult> GetDocument([FromBody] GetNoteDocumentRequest request)
        => this.RequestAsync()
            .For<NoteDocumentDto>()
            .With(request);

    [HttpPost("get-history")]
    public Task<IActionResult> GetHistory([FromBody] GetNoteNodeHistoryRequest request)
        => this.RequestAsync()
            .For<GetNoteNodeHistoryResponse>()
            .With(request);

    [HttpPost("create-folder")]
    public Task<IActionResult> CreateFolder([FromBody] CreateNoteFolderRequest request)
        => this.RequestAsync()
            .For<NoteTreeNodeDto>()
            .With(request);

    [HttpPost("create-document")]
    public Task<IActionResult> CreateDocument([FromBody] CreateNoteDocumentRequest request)
        => this.RequestAsync()
            .For<NoteDocumentDto>()
            .With(request);

    [HttpPost("update-document")]
    public Task<IActionResult> UpdateDocument([FromBody] UpdateNoteDocumentRequest request)
        => this.RequestAsync()
            .For<NoteDocumentDto>()
            .With(request);

    [HttpPost("rename-node")]
    public Task<IActionResult> RenameNode([FromBody] RenameNoteNodeRequest request)
        => this.RequestAsync()
            .For<NoteTreeNodeDto>()
            .With(request);

    [HttpPost("move-node")]
    public Task<IActionResult> MoveNode([FromBody] MoveNoteNodeRequest request)
        => this.RequestAsync()
            .For<NoteTreeNodeDto>()
            .With(request);

    [HttpPost("archive-node")]
    public Task<IActionResult> ArchiveNode([FromBody] ArchiveNoteNodeRequest request)
        => this.RequestAsync()
            .For<NoteTreeNodeDto>()
            .With(request);

    [HttpPost("get-linked-notes")]
    public Task<IActionResult> GetLinkedNotes([FromBody] GetLinkedNotesRequest request)
        => this.RequestAsync()
            .For<GetLinkedNotesResponse>()
            .With(request);

    [HttpPost("create-link")]
    public Task<IActionResult> CreateLink([FromBody] CreateNoteLinkRequest request)
        => this.RequestAsync()
            .For<NoteLinkDto>()
            .With(request);

    [HttpPost("delete-link")]
    public Task<IActionResult> DeleteLink([FromBody] DeleteNoteLinkRequest request)
        => this.RequestAsync(request);
}
