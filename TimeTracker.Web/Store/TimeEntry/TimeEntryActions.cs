using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;

namespace TimeTracker.Web.Store.TimeEntry;

public record struct StartTimeEntryAction();

public record struct StopActiveTimeEntryAction();

public record struct SetActiveTimeEntryAction(TimeEntryDto TimeEntry);

public record struct LoadTimeEntryListAction(int Skip = 1);

public record struct SetTimeEntryListItemsAction(GetListResponse Response);

public record struct SetTimeEntryAction(TimeEntryDto TimeEntry);

public record struct SetTimeEntryIsListLoading(bool IsLoading);
