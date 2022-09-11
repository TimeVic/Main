using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;

namespace TimeTracker.Web.Store.Project;

public record struct LoadProjectListAction(int Skip = 1);

public record struct SetProjectListItemsAction(GetListResponse Response);

public record struct SetProjectIsListLoading(bool IsLoading);
