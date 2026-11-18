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

public class GetContentRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<GetNoteContentRequest, NoteContentDto>
{
    public GetContentRequestHandler(
        IMapper mapper,
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        INoteService noteService
    ) : base(mapper, apiRequestService, userDao, securityManager, noteService)
    {
    }

    public async Task<NoteContentDto> ExecuteAsync(GetNoteContentRequest request)
    {
        var context = await GetWorkspaceContextAsync(AccessLevel.Read);
        var content = await NoteService.GetContentAsync(context.Workspace, context.User, request.ContentId);
        return Mapper.Map<NoteContentDto>(content);
    }
}
