using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Notes.Actions;

public class UpdateContentRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<UpdateNoteContentRequest, NoteContentDto>
{
    public UpdateContentRequestHandler(
        IMapper mapper,
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        INoteService noteService
    ) : base(mapper, apiRequestService, userDao, securityManager, noteService)
    {
    }

    public async Task<NoteContentDto> ExecuteAsync(UpdateNoteContentRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        var content = await NoteService.UpdateContentAsync(
            context.Workspace,
            context.User,
            request.NoteId,
            request.MarkdownContent
        );
        return Mapper.Map<NoteContentDto>(content);
    }
}
