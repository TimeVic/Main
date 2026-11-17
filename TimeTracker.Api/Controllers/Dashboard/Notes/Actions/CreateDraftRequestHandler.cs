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

public class CreateDraftRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<CreateNoteDraftRequest, NoteDocumentDto>
{
    public CreateDraftRequestHandler(
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

    public async Task<NoteDocumentDto> ExecuteAsync(CreateNoteDraftRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        var markdownContent = NormalizeMarkdown(request.MarkdownContent);
        var title = string.IsNullOrWhiteSpace(request.Title)
            ? await GenerateDraftTitleAsync(context.Workspace, context.User, markdownContent)
            : NormalizeTitle(request.Title);

        // Draft notes are always Private and never nested under folders.
        var draft = await NoteDao.CreateNodeAsync(
            context.Workspace,
            parent: null,
            context.User,
            NoteNodeType.Document,
            title,
            markdownContent,
            NoteVisibility.Private,
            sortOrder: 0,
            isDraft: true
        );

        return Mapper.Map<NoteDocumentDto>(draft);
    }
}
