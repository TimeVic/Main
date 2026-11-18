using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Dto.Notes;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Notes.Actions;

public class CreateDocumentRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<CreateNoteDocumentRequest, NoteDocumentDto>
{
    public CreateDocumentRequestHandler(
        IMapper mapper,
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        INoteService noteService
    ) : base(mapper, apiRequestService, userDao, securityManager, noteService)
    {
    }

    public async Task<NoteDocumentDto> ExecuteAsync(CreateNoteDocumentRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        var links = request.Links?
            .Select(item => new NoteLinkTargetDto
            {
                EntityType = item.EntityType,
                EntityId = item.EntityId
            })
            .ToList();

        var note = await NoteService.CreateDocumentAsync(
            context.Workspace,
            context.User,
            request.Title,
            request.Visibility,
            request.ParentId,
            request.SortOrder,
            links
        );

        return Mapper.Map<NoteDocumentDto>(note);
    }
}
