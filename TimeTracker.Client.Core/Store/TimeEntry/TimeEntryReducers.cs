using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants;
// using TimeTracker.Client.Core.Store.Tasks;

namespace TimeTracker.Client.Core.Store.TimeEntry;

public class TimeEntryReducers
{
    [ReducerMethod]
    public static TimeEntryState SetSelectedPageActionReducer(TimeEntryState state, SetSelectedPageAction action)
    {
        return state with
        {
            SelectedPage = action.SelectedPage
        };
    }
    
    [ReducerMethod]
    public static TimeEntryState SetFilteredSelectedPageActionReducer(TimeEntryState state, SetFilteredSelectedPageAction action)
    {
        return state with
        {
            FilteredSelectedPage = action.SelectedPage
        };
    }
    
    [ReducerMethod]
    public static TimeEntryState SetIsTimeEntryProcessingReducer(TimeEntryState state, SetIsTimeEntryProcessingAction action)
    {
        return state with
        {
            IsTimeEntryProcessing = action.IsProcessing
        };
    }
    
    [ReducerMethod]
    public static TimeEntryState SetActiveTimeEntryActionReducer(TimeEntryState state, SetActiveTimeEntryAction action)
    {
        return state with
        {
            ActiveEntry = action.TimeEntry
        };
    }
    
    [ReducerMethod]
    public static TimeEntryState SetTimeEntryListItemsActionReducer(TimeEntryState state, SetTimeEntryListItemsAction action)
    {
        return state with
        {
            List = action.Response.List.Items,
            TotalCount = action.Response.List.TotalCount,
            TotalPages = action.Response.List.TotalPages,
            HasMoreItems = action.Response.List.IsHasMore
        };
    }

    [ReducerMethod]
    public static TimeEntryState AddTimeEntryToListActionReducer(TimeEntryState state, AddTimeEntryToListAction action)
    {
        var existingEntry = state.List.FirstOrDefault(item => item.Id == action.TimeEntry.Id);
        if (existingEntry == null && !IsInCurrentPagePeriod(state, action.TimeEntry))
        {
            return state;
        }

        var list = state.List
            .Where(item => item.Id != action.TimeEntry.Id)
            .Append(action.TimeEntry)
            .OrderByDescending(item => item.StartTime)
            .ToList();

        var isNewDay = existingEntry == null
            && !state.List.Any(item => item.StartTimeOffset.Date == action.TimeEntry.StartTimeOffset.Date);
        var totalCount = isNewDay ? state.TotalCount + 1 : state.TotalCount;

        return state with
        {
            List = list,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(
                (decimal)totalCount / GlobalConstants.TimeEntryGroupedByDayPageSize
            )
        };
    }
    
    [ReducerMethod]
    public static TimeEntryState SetTimeEntryIsListLoadingReducer(TimeEntryState state, SetTimeEntryIsListLoading action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }

    private static bool IsInCurrentPagePeriod(TimeEntryState state, TimeEntryDto timeEntry)
    {
        var currentPageDates = state.List
            .Select(item => item.StartTimeOffset.Date)
            .Distinct()
            .ToList();
        if (currentPageDates.Contains(timeEntry.StartTimeOffset.Date))
        {
            return true;
        }

        if (!currentPageDates.Any())
        {
            return state.SelectedPage == 1;
        }

        return timeEntry.StartTimeOffset.Date >= currentPageDates.Min()
            && timeEntry.StartTimeOffset.Date <= currentPageDates.Max();
    }

    [ReducerMethod]
    public static TimeEntryState UpdateTimeEntryActionReducer(TimeEntryState state, UpdateTimeEntryAction action)
    {
        state.List = state.List.Select(item =>
        {
            if (item.Id == action.TimeEntry.Id)
                item.UpdateFrom(action.TimeEntry);
            return item;
        }).ToList();
        state.FilteredList = state.FilteredList.Select(item =>
        {
            if (item.Id == action.TimeEntry.Id)
                item.UpdateFrom(action.TimeEntry);
            return item;
        }).ToList();
        if (state.ActiveEntry != null && state.ActiveEntry?.Id == action.TimeEntry.Id)
        {
            state.ActiveEntry.UpdateFrom(action.TimeEntry);
        }
        return state;
    }
    
    [ReducerMethod]
    public static TimeEntryState DeleteTimeEntryFromListActionReducer(TimeEntryState state, DeleteTimeEntryFromListAction action)
    {
        return state with
        {
            List = state.List.Where(item => item.Id != action.EntryId).ToList(),
            TotalCount = state.TotalCount > 0 ? --state.TotalCount : 0
        };
    }
    
    #region Filtered List
    
    [ReducerMethod]
    public static TimeEntryState SetTimeEntryFilterReducer(TimeEntryState state, SetTimeEntryFilterAction action)
    {
        return state with
        {
            Filter = action.Filter
        };
    }
    
    [ReducerMethod]
    public static TimeEntryState SetTimeEntryFilteredListItemsActionReducer(TimeEntryState state, SetTimeEntryFilteredListItemsAction action)
    {
        return state with
        {
            FilteredList = action.Response.Items.Where(item => !item.IsActive).ToList(),
            FilteredTotalCount = action.Response.TotalCount,
            FilteredTotalPages = action.Response.TotalPages,
            FilteredHasMoreItems = action.Response.IsHasMore,
        };
    }
    
    #endregion
}
