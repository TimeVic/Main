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

public class PromoteDraftRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<PromoteNoteDraftRequest, NoteDocumentDto>
{
    public PromoteDraftRequestHandler(
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

    public async Task<NoteDocumentDto> ExecuteAsync(PromoteNoteDraftRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        var draft = await GetNoteAsync(context.Workspace, context.User, request.NoteId, AccessLevel.Write);
        EnsureDraft(draft);
        await EnsureCanUseVisibilityAsync(context.Workspace, context.User, request.Visibility);

        var parent = await GetValidParentAsync(context.Workspace, context.User, request.ParentId);
        EnsureMatchingParentVisibility(parent, request.Visibility);

        var now = DateTime.UtcNow;
        if (request.MarkdownContent != null)
        {
            var nextMarkdown = NormalizeMarkdown(request.MarkdownContent);
            if (draft.LastContent == null || draft.LastContent.MarkdownContent != nextMarkdown)
            {
                await NoteDao.CreateContentAsync(draft, nextMarkdown, now);
            }
        }

        draft.IsDraft = false;
        draft.Visibility = request.Visibility;
        draft.Parent = parent;
        draft.Title = NormalizeTitle(request.Title);
        draft.SortOrder = await ResolveSortOrderAsync(context.Workspace, parent, request.SortOrder);
        SetUpdatedBy(draft, context.User, now);

        await NoteDao.SaveNodeAsync(draft);

        return Mapper.Map<NoteDocumentDto>(draft);
    }
}
