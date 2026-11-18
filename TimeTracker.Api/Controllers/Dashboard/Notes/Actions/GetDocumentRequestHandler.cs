using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Notes.Actions;

public class GetDocumentRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<GetNoteDocumentRequest, NoteDocumentDto>
{
    public GetDocumentRequestHandler(
        IMapper mapper,
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        INoteService noteService
    ) : base(mapper, apiRequestService, userDao, securityManager, noteService)
    {
    }

    public async Task<NoteDocumentDto> ExecuteAsync(GetNoteDocumentRequest request)
    {
        var context = await GetWorkspaceContextAsync(AccessLevel.Read);
        var note = await NoteService.GetDocumentAsync(context.Workspace, context.User, request.NoteId);
        return Mapper.Map<NoteDocumentDto>(note);
    }
}
