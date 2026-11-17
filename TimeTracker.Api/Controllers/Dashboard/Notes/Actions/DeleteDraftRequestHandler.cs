using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Notes;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Notes.Actions;

public class DeleteDraftRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<DeleteNoteDraftRequest>
{
    public DeleteDraftRequestHandler(
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

    public async Task ExecuteAsync(DeleteNoteDraftRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        var draft = await GetNoteAsync(context.Workspace, context.User, request.NoteId, AccessLevel.Write);
        EnsureDraft(draft);

        await NoteDao.DeleteDraftAsync(draft);
    }
}
