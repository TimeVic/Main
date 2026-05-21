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

public class ArchiveNodeRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<ArchiveNoteNodeRequest, NoteTreeNodeDto>
{
    public ArchiveNodeRequestHandler(
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

    public async Task<NoteTreeNodeDto> ExecuteAsync(ArchiveNoteNodeRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        var note = await GetNoteAsync(context.Workspace, context.User, request.NoteId, AccessLevel.Write);
        var allNodes = await NoteDao.GetWorkspaceNodesAsync(context.Workspace);
        var now = DateTime.UtcNow;

        // Folder archive is recursive so archived folders cannot leave active orphaned children in the tree.
        foreach (var node in GetNodeWithDescendants(note, allNodes))
        {
            await SecurityManager.CheckAccess(AccessLevel.Write, context.User, node);
            node.ArchivedAt ??= now;
            SetUpdatedBy(node, context.User, now);
            await NoteDao.SaveNodeAsync(node);
        }

        return Mapper.Map<NoteTreeNodeDto>(note);
    }
}
