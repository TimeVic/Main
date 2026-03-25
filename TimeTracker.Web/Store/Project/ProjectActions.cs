using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;

namespace TimeTracker.Web.Store.Project;

public record struct LoadListAction(bool IsReload = false);

public record struct SetListItemsAction(GetListResponse Response);

public record struct SetListItemAction(ProjectDto Project);

public record struct SetSelectedAction(ProjectDto Project);

public record struct SetProjectIsListLoading(bool IsLoading);
