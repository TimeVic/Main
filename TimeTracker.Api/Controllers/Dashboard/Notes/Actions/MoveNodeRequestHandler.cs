using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Notes.Actions;

public class MoveNodeRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<MoveNoteNodeRequest, NoteTreeNodeDto>
{
    public MoveNodeRequestHandler(
        IMapper mapper,
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        INoteService noteService
    ) : base(mapper, apiRequestService, userDao, securityManager, noteService)
    {
    }

    public async Task<NoteTreeNodeDto> ExecuteAsync(MoveNoteNodeRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        var note = await NoteService.MoveNodeAsync(
            context.Workspace,
            context.User,
            request.NoteId,
            request.ParentId,
            request.SortOrder
        );
        return Mapper.Map<NoteTreeNodeDto>(note);
    }
}
