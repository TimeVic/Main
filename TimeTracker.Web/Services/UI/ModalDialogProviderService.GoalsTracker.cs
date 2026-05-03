using MudBlazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Web.Pages.Dashboard.GoalsTracker.Parts;
using TimeTracker.Web.Pages.Dashboard.Tag.Parts;

namespace TimeTracker.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public async Task ShowGoalsTrackerAddModal()
    {
        await _mudDialogService.ShowAsync<AddGoalModalForm>(
            "Create New",
            new MudBlazor.DialogOptions()
            {
                CloseButton = true,
                MaxWidth = MaxWidth.Medium
            }
        );
    }
    
    public async Task ShowGoalsTrackerChangePositionsModal(GoalsTrackerDto tracker)
    {
        var parameters = new DialogParameters<ChangePositionModal>
        {
            {context => context.Tracker, tracker}
        };
        await _mudDialogService.ShowAsync<ChangePositionModal>(
            $"Change positions", 
            parameters,
            new MudBlazor.DialogOptions()
            {
                CloseButton = true,
                MaxWidth = MaxWidth.Medium
            }
        );
    }
    
    public async Task ShowGoalsTrackerUpdateItemModal(GoalsTrackerItemDto item)
    {
        var parameters = new DialogParameters<UpdateGoalModalForm>
        {
            {context => context.Item, item}
        };
        await _mudDialogService.ShowAsync<UpdateGoalModalForm>(
            $"Update goal", 
            parameters,
            new MudBlazor.DialogOptions()
            {
                CloseButton = true,
                MaxWidth = MaxWidth.Medium
            }
        );
    }
}
