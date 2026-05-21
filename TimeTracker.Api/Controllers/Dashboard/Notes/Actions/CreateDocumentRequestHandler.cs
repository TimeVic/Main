using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Notes;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Notes.Actions;

public class CreateDocumentRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<CreateNoteDocumentRequest, NoteDocumentDto>
{
    public CreateDocumentRequestHandler(
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

    public async Task<NoteDocumentDto> ExecuteAsync(CreateNoteDocumentRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        var parent = await GetValidParentAsync(context.Workspace, context.User, request.ParentId);
        var links = request.Links ?? new List<NoteLinkRequestDto>();
        EnsureNoDuplicateLinks(links);

        foreach (var link in links)
        {
            await EnsureLinkedEntityExistsAsync(context.Workspace, link.EntityType, link.EntityId);
        }

        var note = await NoteDao.CreateNodeAsync(
            context.Workspace,
            parent,
            context.User,
            NoteNodeType.Document,
            NormalizeTitle(request.Title),
            NormalizeMarkdown(request.MarkdownContent),
            request.Visibility,
            await ResolveSortOrderAsync(context.Workspace, parent, request.SortOrder)
        );

        foreach (var link in links)
        {
            await NoteDao.CreateLinkAsync(context.Workspace, note, context.User, link.EntityType, link.EntityId);
        }

        return Mapper.Map<NoteDocumentDto>(note);
    }
}
