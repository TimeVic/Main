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

public class UpdateContentRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<UpdateNoteContentRequest, NoteContentDto>
{
    public UpdateContentRequestHandler(
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

    public async Task<NoteContentDto> ExecuteAsync(UpdateNoteContentRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        var note = await GetNoteAsync(context.Workspace, context.User, request.NoteId, AccessLevel.Write);
        EnsureDocument(note);

        var now = DateTime.UtcNow;
        var content = await NoteDao.CreateContentAsync(note, NormalizeMarkdown(request.MarkdownContent), now);
        SetUpdatedBy(note, context.User, now);
        await NoteDao.SaveNodeAsync(note);

        return Mapper.Map<NoteContentDto>(content);
    }
}
