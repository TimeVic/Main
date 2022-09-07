using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.TimeEntry;

public record struct StartTimeEntryAction();

public record struct StopActiveTimeEntryAction();

public record struct SetActiveTimeEntryAction(TimeEntryDto TimeEntry);
