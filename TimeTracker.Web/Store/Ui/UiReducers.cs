using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.Ui;

public class UiReducers
{

    [ReducerMethod]
    public static UiState SetListItemsActionReducer(UiState state, ToggleMainMenuAction action)
    {
        return state with
        {
            IsMainMenuOpened = !state.IsMainMenuOpened
        };
    }
}
