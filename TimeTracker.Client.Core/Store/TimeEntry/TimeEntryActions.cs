using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;

namespace TimeTracker.Client.Core.Store.TimeEntry;

public record struct StartTimeEntryAction(
    bool? IsBillable = null,
    ProjectDto? Project = null,
    string? Description = null,
    decimal? HourlyRate = null,
    TaskDto? InternalTask = null
);

public record struct StopActiveTimeEntryAction();

public record struct SetActiveTimeEntryAction(TimeEntryDto TimeEntry);

public record struct LoadListAction();

public record struct SetTimeEntryListItemsAction(GetListResponse Response);

public record struct SaveTimeEntryAction(TimeEntryDto TimeEntry, bool IsSetProjectDefaults = false);

public record struct UpdateTimeEntryAction(TimeEntryDto TimeEntry);

public record struct SetTimeEntryIsListLoading(bool IsLoading);

public record struct DeleteTimeEntryAction(Guid EntryId);

public record struct DeleteTimeEntryFromListAction(Guid EntryId);

public record struct SetIsTimeEntryProcessingAction(bool IsProcessing);

public record struct SetSelectedPageAction(int SelectedPage);

public record struct SetFilteredSelectedPageAction(int SelectedPage);

#region Filtered

public record struct LoadTimeEntryFilteredListAction();

public record struct SetTimeEntryFilterAction(TimeEntryFilterState Filter);

public record struct SetTimeEntryFilteredListItemsAction(GetFilteredListResponse Response);

#endregion
