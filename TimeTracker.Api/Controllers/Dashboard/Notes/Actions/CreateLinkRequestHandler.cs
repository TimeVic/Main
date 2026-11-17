using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Notes.Actions;

public class CreateLinkRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<CreateNoteLinkRequest, NoteLinkDto>
{
    public CreateLinkRequestHandler(
        IMapper mapper,
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        INoteService noteService
    ) : base(mapper, apiRequestService, userDao, securityManager, noteService)
    {
    }

    public async Task<NoteLinkDto> ExecuteAsync(CreateNoteLinkRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        var link = await NoteService.CreateLinkAsync(
            context.Workspace,
            context.User,
            request.NoteId,
            request.EntityType,
            request.EntityId
        );
        return Mapper.Map<NoteLinkDto>(link);
    }
}
