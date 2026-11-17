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

public class GetLinkedNotesRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<GetLinkedNotesRequest, GetLinkedNotesResponse>
{
    public GetLinkedNotesRequestHandler(
        IMapper mapper,
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        INoteService noteService
    ) : base(mapper, apiRequestService, userDao, securityManager, noteService)
    {
    }

    public async Task<GetLinkedNotesResponse> ExecuteAsync(GetLinkedNotesRequest request)
    {
        var context = await GetWorkspaceContextAsync(AccessLevel.Read);
        var availableNotes = await NoteService.GetLinkedNotesAsync(
            context.Workspace,
            context.User,
            request.EntityType,
            request.EntityId
        );

        return new GetLinkedNotesResponse
        {
            Notes = Mapper.Map<ICollection<NoteTreeNodeDto>>(availableNotes)
        };
    }
}
