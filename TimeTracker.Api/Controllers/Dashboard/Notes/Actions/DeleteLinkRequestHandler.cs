using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Notes.Actions;

public class DeleteLinkRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<DeleteNoteLinkRequest>
{
    public DeleteLinkRequestHandler(
        IMapper mapper,
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        INoteService noteService
    ) : base(mapper, apiRequestService, userDao, securityManager, noteService)
    {
    }

    public async Task ExecuteAsync(DeleteNoteLinkRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        await NoteService.DeleteLinkAsync(context.Workspace, context.User, request.NoteId, request.LinkId);
    }
}
