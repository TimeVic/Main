using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;

namespace TimeTracker.Web.Store.Common;

public record struct InitializeAppAction();

public record struct SetIsAppInitializedAction(bool IsInitialized);

public record struct SetIsWorkspaceInitializedAction(bool IsInitialized);
