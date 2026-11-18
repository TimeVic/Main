using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Notes.Actions;

public class GetTreeRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<GetNotesTreeRequest, GetNotesTreeResponse>
{
    public GetTreeRequestHandler(
        IMapper mapper,
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        INoteService noteService
    ) : base(mapper, apiRequestService, userDao, securityManager, noteService)
    {
    }

    public async Task<GetNotesTreeResponse> ExecuteAsync(GetNotesTreeRequest request)
    {
        var context = await GetWorkspaceContextAsync(AccessLevel.Read);
        var visibility = request.Visibility ?? NoteVisibility.Private;
        var result = await NoteService.GetTreeAsync(
            context.Workspace,
            context.User,
            request.IncludeArchived,
            visibility
        );

        return new GetNotesTreeResponse
        {
            Nodes = Mapper.Map<ICollection<NoteTreeNodeDto>>(result.Nodes),
            Drafts = Mapper.Map<ICollection<NoteTreeNodeDto>>(result.Drafts)
        };
    }
}
