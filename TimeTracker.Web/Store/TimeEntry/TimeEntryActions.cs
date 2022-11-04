using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;

namespace TimeTracker.Web.Store.TimeEntry;

public record struct StartTimeEntryAction(
    bool? IsBillable = null,
    ProjectDto? Project = null,
    string? TaskId = null,
    string? Description = null,
    decimal? HourlyRate = null
);

public record struct StopActiveTimeEntryAction();

public record struct SetActiveTimeEntryAction(TimeEntryDto TimeEntry);

public record struct LoadTimeEntryListAction(int Skip = 1);

public record struct SetTimeEntryListItemsAction(GetListResponse Response);

public record struct SaveTimeEntryAction(TimeEntryDto TimeEntry, bool IsSetProjectDefaults = false);

public record struct UpdateTimeEntryAction(TimeEntryDto TimeEntry);

public record struct SetTimeEntryIsListLoading(bool IsLoading);

public record struct DeleteTimeEntryAction(long EntryId);

public record struct DeleteTimeEntryFromListAction(long EntryId);

#region Filtered

public record struct LoadTimeEntryFilteredListAction(int Skip = 1);

public record struct SetTimeEntryFilterAction(TimeEntryFilterState Filter);

public record struct SetTimeEntryFilteredListItemsAction(GetFilteredListResponse Response);

#endregion
