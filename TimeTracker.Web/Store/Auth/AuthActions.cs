using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;

namespace TimeTracker.Web.Store.Auth;

public record struct LogoutAction();

public record struct LoginAction(string Jwt, UserDto User, WorkspaceDto Workspace)
{
    public LoginAction(AuthState state) : this(state.Jwt, state.User, state.Workspace)
    {
    }
}

public record struct SetWorkspaceAction(WorkspaceDto Workspace);
