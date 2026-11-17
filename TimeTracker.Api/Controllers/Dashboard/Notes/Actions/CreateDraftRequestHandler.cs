using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Notes.Actions;

public class CreateDraftRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<CreateNoteDraftRequest, NoteDocumentDto>
{
    public CreateDraftRequestHandler(
        IMapper mapper,
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        INoteService noteService
    ) : base(mapper, apiRequestService, userDao, securityManager, noteService)
    {
    }

    public async Task<NoteDocumentDto> ExecuteAsync(CreateNoteDraftRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        var draft = await NoteService.CreateDraftAsync(
            context.Workspace,
            context.User,
            request.Title,
            request.MarkdownContent
        );
        return Mapper.Map<NoteDocumentDto>(draft);
    }
}
