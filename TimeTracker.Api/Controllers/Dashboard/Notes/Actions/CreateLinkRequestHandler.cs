using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Notes;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Notes.Actions;

public class CreateLinkRequestHandler : NoteRequestHandlerBase, IAsyncRequestHandler<CreateNoteLinkRequest, NoteLinkDto>
{
    public CreateLinkRequestHandler(
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

    public async Task<NoteLinkDto> ExecuteAsync(CreateNoteLinkRequest request)
    {
        var context = await GetWorkspaceContextAsync();
        var note = await GetNoteAsync(context.Workspace, context.User, request.NoteId, AccessLevel.Write);
        EnsureDocument(note);
        await EnsureLinkedEntityExistsAsync(context.Workspace, request.EntityType, request.EntityId);

        if (await NoteDao.IsLinkExistsAsync(note, request.EntityType, request.EntityId))
        {
            throw new RecordIsExistsException("Note link already exists");
        }

        var link = await NoteDao.CreateLinkAsync(
            context.Workspace,
            note,
            context.User,
            request.EntityType,
            request.EntityId
        );
        return Mapper.Map<NoteLinkDto>(link);
    }
}
