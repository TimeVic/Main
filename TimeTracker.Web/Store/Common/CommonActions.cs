using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;

namespace TimeTracker.Web.Store.Common;

public record struct LoadPersistedDataAction();

public record struct PersistDataAction(bool RedirectToLoginAfterPersist = false);

public record struct SetIsAppInitializedAction(bool IsInitialized);

public record struct SetIsWorkspaceInitializedAction(bool IsInitialized);
