﻿using Fluxor;
using TimeTracker.Web.Core.Helpers;

namespace TimeTracker.Web.Store.TimeEntry;

public class TimeEntryReducers
{
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
            HasMoreItems = action.Response.List.IsHasMore,
            ActiveEntry = action.Response.ActiveTimeEntry
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
    
    [ReducerMethod]
    public static TimeEntryState UpdateTimeEntryActionReducer(TimeEntryState state, UpdateTimeEntryAction action)
    {
        var timeEntry = state.List.FirstOrDefault(item => item.Id == action.TimeEntry.Id);
        if (timeEntry != null)
        {
            timeEntry.UpdateFrom(action.TimeEntry);
        }
        if (state.ActiveEntry != null && state.ActiveEntry?.Id == action.TimeEntry.Id)
        {
            state.ActiveEntry.UpdateFrom(action.TimeEntry);
        }

        return state;
    }
}
