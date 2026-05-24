using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Client.Core.Store.Tag;

public class ProjectReducers
{
    [ReducerMethod]
    public static TagState SetIsSavingActionReducer(TagState state, SetIsSavingAction action)
    {
        return state with
        {
            IsSaving = action.IsSaving
        };
    }

    [ReducerMethod]
    public static TagState SetListItemsActionReducer(TagState state, SetListItemsAction action)
    {
        return state with
        {
            List = action.Response.Items,
            TotalCount = action.Response.TotalCount,
            TotalPages = action.Response.TotalPages,
            HasMoreItems = action.Response.IsHasMore,
            IsLoaded = true
        };
    }
    
    [ReducerMethod]
    public static TagState SetListItemActionReducer(TagState state, SetListItemAction action)
    {
        var listItems = state.List.Select(item =>
        {
            if (item.Id == action.Tag.Id)
            {
                return action.Tag;
            }

            return item;
        }).ToList();
        if (listItems.All(item => item.Id != action.Tag.Id))
        {
            listItems.Insert(0, action.Tag);
        }
        return state with
        {
            List = listItems
        };
    }
    
    [ReducerMethod]
    public static TagState DeleteListItemReducer(TagState state, DeleteListItemAction action)
    {
        var listItems = state.List.Where(item => item.Id != action.TagId).ToList();
        return state with
        {
            List = listItems
        };
    }

    [ReducerMethod]
    public static TagState SetIsListLoadingReducer(TagState state, SetIsListLoading action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
}
