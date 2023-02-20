using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.Tag;

public class ProjectReducers
{

    [ReducerMethod]
    public static TagState SetProjectListItemsActionReducer(TagState state, SetListItemsAction action)
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
    public static TagState SetProjectIsListLoadingReducer(TagState state, SetIsListLoading action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
    
    #region Add new item
    
    [ReducerMethod(typeof(AddEmptyListItemAction))]
    public static TagState AddEmptyProjectListItemActionAction(TagState state)
    {
        var newList = state.SortedList.ToList();
        newList.Add(new TagDto()
        {
            Id = 0,
            Name = ""
        });
        return state with
        {
            List = newList,
            TotalCount = ++state.TotalCount
        };
    }
    
    [ReducerMethod(typeof(RemoveEmptyListItemAction))]
    public static TagState RemoveEmptyProjectListItemActionAction(TagState state)
    {
        return state with
        {
            List = state.List.Where(item => item.Id != 0).ToList(),
            TotalCount = --state.TotalCount
        };
    }
    
    #endregion
}
