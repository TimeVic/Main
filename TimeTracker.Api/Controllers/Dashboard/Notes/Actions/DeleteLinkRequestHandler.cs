using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using AutoMapper;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Notes;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
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
        INoteDao noteDao,
        IClientDao clientDao,
        IProjectDao projectDao,
        ITaskDao taskDao
    ) : base(mapper, apiRequestService, userDao, securityManager, noteDao, clientDao, projectDao, taskDao)
    {
    }

    public async Task ExecuteAsync(DeleteNoteLinkRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        var note = await GetNoteAsync(context.Workspace, context.User, request.NoteId, AccessLevel.Write);
        EnsureDocument(note);

        var link = await NoteDao.GetLinkByIdAsync(context.Workspace, note, request.LinkId);
        RecordNotFoundException.ThrowIfNull(link);
        await NoteDao.DeleteLinkAsync(link);
    }
}
