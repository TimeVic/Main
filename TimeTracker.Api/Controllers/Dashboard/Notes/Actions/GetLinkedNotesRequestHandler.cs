using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Notes;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Notes.Actions;

public class GetLinkedNotesRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<GetLinkedNotesRequest, GetLinkedNotesResponse>
{
    public GetLinkedNotesRequestHandler(
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

    public async Task<GetLinkedNotesResponse> ExecuteAsync(GetLinkedNotesRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        await EnsureLinkedEntityExistsAsync(context.Workspace, request.EntityType, request.EntityId);

        var links = await NoteDao.GetLinksByEntityAsync(context.Workspace, request.EntityType, request.EntityId);
        var notes = links
            .Select(item => item.NoteNode)
            .Where(item => item.Type == NoteNodeType.Document && item.ArchivedAt == null)
            .DistinctBy(item => item.Id)
            .ToList();
        var availableNotes = await GetAvailableNotesAsync(context.User, notes, AccessLevel.Read);

        return new GetLinkedNotesResponse
        {
            Notes = Mapper.Map<ICollection<NoteTreeNodeDto>>(availableNotes)
        };
    }
}
