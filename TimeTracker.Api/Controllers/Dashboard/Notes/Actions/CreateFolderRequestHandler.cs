using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Notes;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Notes.Actions;

public class CreateFolderRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<CreateNoteFolderRequest, NoteTreeNodeDto>
{
    public CreateFolderRequestHandler(
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

    public async Task<NoteTreeNodeDto> ExecuteAsync(CreateNoteFolderRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        await EnsureCanUseVisibilityAsync(context.Workspace, context.User, request.Visibility);

        var parent = await GetValidParentAsync(context.Workspace, context.User, request.ParentId);
        EnsureMatchingParentVisibility(parent, request.Visibility);
        var note = await NoteDao.CreateNodeAsync(
            context.Workspace,
            parent,
            context.User,
            NoteNodeType.Folder,
            NormalizeTitle(request.Title),
            null,
            request.Visibility,
            await ResolveSortOrderAsync(context.Workspace, parent, request.SortOrder)
        );

        return Mapper.Map<NoteTreeNodeDto>(note);
    }
}
