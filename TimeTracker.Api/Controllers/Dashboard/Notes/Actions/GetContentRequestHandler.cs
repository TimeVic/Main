using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Notes;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
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
        INoteDao noteDao,
        IClientDao clientDao,
        IProjectDao projectDao,
        ITaskDao taskDao
    ) : base(mapper, apiRequestService, userDao, securityManager, noteDao, clientDao, projectDao, taskDao)
    {
    }

    public async Task<NoteContentDto> ExecuteAsync(GetNoteContentRequest request)
    {
        var context = await GetWorkspaceContextAsync(AccessLevel.Read);
        var content = await NoteDao.GetContentByIdAsync(context.Workspace, request.ContentId);
        RecordNotFoundException.ThrowIfNull(content);
        EnsureDocument(content.NoteNode);
        await SecurityManager.CheckAccess(AccessLevel.Read, context.User, content.NoteNode);
        return Mapper.Map<NoteContentDto>(content);
    }
}
