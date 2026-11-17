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

public class UpdateDraftRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<UpdateNoteDraftRequest, NoteDocumentDto>
{
    public UpdateDraftRequestHandler(
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

    public async Task<NoteDocumentDto> ExecuteAsync(UpdateNoteDraftRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        var draft = await GetNoteAsync(context.Workspace, context.User, request.NoteId, AccessLevel.Write);
        EnsureDraft(draft);

        var now = DateTime.UtcNow;
        var currentMarkdown = draft.LastContent?.MarkdownContent ?? string.Empty;
        var nextMarkdown = request.MarkdownContent != null
            ? NormalizeMarkdown(request.MarkdownContent)
            : currentMarkdown;

        if (request.Title != null)
        {
            draft.Title = string.IsNullOrWhiteSpace(request.Title)
                ? await GenerateDraftTitleAsync(context.Workspace, context.User, nextMarkdown)
                : NormalizeTitle(request.Title);
        }

        if (request.MarkdownContent != null
            && (draft.LastContent == null || draft.LastContent.MarkdownContent != nextMarkdown))
        {
            await NoteDao.CreateContentAsync(draft, nextMarkdown, now);
        }

        SetUpdatedBy(draft, context.User, now);
        await NoteDao.SaveNodeAsync(draft);

        return Mapper.Map<NoteDocumentDto>(draft);
    }
}
