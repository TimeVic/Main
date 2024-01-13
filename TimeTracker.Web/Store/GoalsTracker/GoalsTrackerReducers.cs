using Fluxor;
using TimeTracker.Web.Store.GoalsTracker;

namespace TimeTracker.Web.Store.GoalsTracker;

public class GoalsTrackerReducers
{

    [ReducerMethod]
    public static GoalsTrackerState SetTrackerActionReducer(GoalsTrackerState state, SetTrackerAction action)
    {
        return state with
        {
            CurrentTracker = action.Tracker
        };
    }
    
    [ReducerMethod]
    public static GoalsTrackerState SetIsListLoadingActionReducer(GoalsTrackerState state, SetIsListLoadingAction action)
    {
        return state with
        {
            IsLoading = action.IsLoading
        };
    }
    
    [ReducerMethod]
    public static GoalsTrackerState SetCompletionItemsActionReducer(GoalsTrackerState state, SetCompletionItemsAction action)
    {
        var currentTracker = state.CurrentTracker;
        if (currentTracker == null)
        {
            return state;
        }
        currentTracker.Items = state.CurrentTracker.Items.Select(item =>
        {
            if (item.Id == action.Item.Id)
            {
                item.CompletionMarkers = action.CompletionMarkers;
            }

            return item;
        }).ToList();
        return state with
        {
            CurrentTracker = currentTracker
        };
    }
    
    [ReducerMethod]
    public static GoalsTrackerState SetCompletionItemActionReducer(GoalsTrackerState state, SetCompletionItemAction action)
    {
        var currentTracker = state.CurrentTracker;
        if (currentTracker == null)
        {
            return state;
        }
        currentTracker.Items = state.CurrentTracker.Items.Select(item =>
        {
            if (item.Id == action.Item.Id)
            {
                var isUpdated = false;
                item.CompletionMarkers = item.CompletionMarkers.Select(item2 =>
                {
                    if (item2.Id == action.CompletionMarker.Id)
                    {
                        isUpdated = true;
                        return action.CompletionMarker;
                    }

                    return item2;
                }).ToList();
                if (!isUpdated)
                {
                    item.CompletionMarkers.Add(action.CompletionMarker);
                }
            }
            return item;
        }).ToList();
        return state with
        {
            CurrentTracker = currentTracker
        };
    }
    
    [ReducerMethod]
    public static GoalsTrackerState SetGoalsTrackerItemActionReducer(GoalsTrackerState state, SetGoalsTrackerItemAction action)
    {
        var currentTracker = state.CurrentTracker;
        if (currentTracker == null)
        {
            return state;
        }
        if (currentTracker.Items.All(item => item.Id != action.Item.Id))
        {
            currentTracker.Items.Add(action.Item);
        }

        currentTracker.Items = state.CurrentTracker.Items.Select(item =>
        {
            if (item.Id == action.Item.Id)
            {
                return action.Item;
            }
            return item;
        }).ToList();
        return state with
        {
            CurrentTracker = currentTracker
        };
    }
}
