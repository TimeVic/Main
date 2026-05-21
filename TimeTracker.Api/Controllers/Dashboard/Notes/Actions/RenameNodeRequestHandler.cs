using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Notes;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Notes.Actions;

public class RenameNodeRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<RenameNoteNodeRequest, NoteTreeNodeDto>
{
    public RenameNodeRequestHandler(
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

    public async Task<NoteTreeNodeDto> ExecuteAsync(RenameNoteNodeRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        var note = await GetNoteAsync(context.Workspace, context.User, request.NoteId, AccessLevel.Write);

        note.Title = NormalizeTitle(request.Title);
        SetUpdatedBy(note, context.User);
        await NoteDao.SaveNodeAsync(note);

        return Mapper.Map<NoteTreeNodeDto>(note);
    }
}
