using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Services.Users;

public class UserDtoBuilder : IUserDtoBuilder
{
    private readonly IMapper _mapper;
    private readonly IUserDao _userDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public UserDtoBuilder(
        IMapper mapper,
        IUserDao userDao,
        IWorkspaceAccessService workspaceAccessService
    )
    {
        _mapper = mapper;
        _userDao = userDao;
        _workspaceAccessService = workspaceAccessService;
    }

    public async Task<UserDto> BuildAsync(UserEntity user)
    {
        var userDto = _mapper.Map<UserDto>(user);
        var defaultWorkspace = await _userDao.GetDefaultWorkspace(user);
        var selectedWorkspace = await _userDao.GetSelectedWorkspaceAsync(user);

        userDto.DefaultWorkspace = await MapWorkspaceAsync(user, defaultWorkspace);
        userDto.SelectedWorkspace = await MapWorkspaceAsync(user, selectedWorkspace);

        return userDto;
    }

    private async Task<WorkspaceDto> MapWorkspaceAsync(UserEntity user, WorkspaceEntity workspace)
    {
        var workspaceDto = _mapper.Map<WorkspaceDto>(workspace);
        workspaceDto.CurrentUserAccess = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
        return workspaceDto;
    }
}
