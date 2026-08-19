using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Notes;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Notes.Actions;

public class MoveNodeRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<MoveNoteNodeRequest, NoteTreeNodeDto>
{
    public MoveNodeRequestHandler(
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

    public async Task<NoteTreeNodeDto> ExecuteAsync(MoveNoteNodeRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        var note = await GetNoteAsync(context.Workspace, context.User, request.NoteId, AccessLevel.Write);
        var parent = await GetValidParentAsync(context.Workspace, context.User, request.ParentId);
        EnsureMatchingParentVisibility(parent, note.Visibility);

        if (note.Type == NoteNodeType.Folder && parent != null)
        {
            await EnsureFolderIsNotMovedIntoItselfOrDescendantAsync(context.Workspace, note, parent);
        }

        note.Parent = parent;
        note.SortOrder = await ResolveSortOrderAsync(context.Workspace, parent, request.SortOrder);
        SetUpdatedBy(note, context.User);
        await NoteDao.SaveNodeAsync(note);

        return Mapper.Map<NoteTreeNodeDto>(note);
    }
}
