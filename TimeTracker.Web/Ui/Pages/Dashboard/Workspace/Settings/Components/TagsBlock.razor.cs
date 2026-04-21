using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.Tag;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Workspace.Settings.Components;

public partial class TagsBlock
{
    [Inject]
    private IState<TagState> _state { get; set; }

    private TagDto? _tagToUpdate { get; set; }
    
    private TagDto? _tagToDelete { get; set; }

    private Task OnEdit(TagDto context)
    {
        _tagToUpdate = context;
        return Task.CompletedTask;
    }

    private Task OnDeleteClicked(TagDto context)
    {
        _tagToDelete = context;
        return Task.CompletedTask;
    }
    
    private Task OnConfirmDelete()
    {
        if (_tagToDelete != null)
        {
            Dispatcher.Dispatch(new DeleteItemAction(_tagToDelete));
            _tagToDelete = null;
        }

        return Task.CompletedTask;
    }
    
    private Task OnCloseDeleteConfirmation()
    {
        _tagToDelete = null;
        return Task.CompletedTask;
    }

    private static string GetColorStyle(string color)
    {
        return $"background-color: {color};";
    }
}
