using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Client.Core.Store.Auth;

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

public record struct UpdateUserAction(UserDto User);

public record struct LoadCurrentUserAction();

public record struct SelectWorkspaceAction(WorkspaceDto Workspace);
