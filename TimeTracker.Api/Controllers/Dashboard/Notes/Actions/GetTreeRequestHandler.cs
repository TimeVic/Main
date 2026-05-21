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

public class GetTreeRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<GetNotesTreeRequest, GetNotesTreeResponse>
{
    public GetTreeRequestHandler(
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

    public async Task<GetNotesTreeResponse> ExecuteAsync(GetNotesTreeRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        var nodes = await NoteDao.GetTreeAsync(context.Workspace, request.IncludeArchived);
        var availableNodes = await GetAvailableNotesAsync(context.User, nodes, AccessLevel.Read);

        return new GetNotesTreeResponse
        {
            Nodes = Mapper.Map<ICollection<NoteTreeNodeDto>>(availableNodes)
        };
    }
}
