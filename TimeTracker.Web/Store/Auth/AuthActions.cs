using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;

namespace TimeTracker.Web.Store.Auth;

public record struct LogoutAction();

public record struct LoginAction(
    UserDto User,
    WorkspaceDto Workspace
)
{
    public LoginAction(AuthState state) : this(state.User, state.Workspace)
    {
    }
}

public record struct SetWorkspaceAction(WorkspaceDto Workspace);

public record struct UpdateUserAvatarAction(StoredFileDto? Avatar);
