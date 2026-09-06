using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Store.Tag;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Tags;

public partial class TagsBlock
{
    [Inject]
    private IState<TagState> _state { get; set; }

    [Inject]
    private TimeTracker.Client.Web.Services.UI.IModalDialogProviderService _modalDialogService { get; set; } = null!;

    private async Task OnAdd()
    {
        await _modalDialogService.ShowAddTagModal();
    }

    private async Task OnEdit(TagDto context)
    {
        await _modalDialogService.ShowUpdateTagModal(context);
    }

    private async Task OnDeleteClicked(TagDto context)
    {
        var confirmed = await _modalDialogService.ShowConfirmationAsync(
            string.Format(DashboardLocalizer["TagsBlock_DeleteTagSubtitle"].Value, context.Name),
            DashboardLocalizer["TagsBlock_DeleteTagTitle"].Value,
            confirmText: DashboardLocalizer["Delete"].Value
        );
        if (confirmed)
        {
            Dispatcher.Dispatch(new DeleteItemAction(context));
        }
    }

    private static string GetColorStyle(string color)
    {
        return $"background-color: {color};";
    }
}
