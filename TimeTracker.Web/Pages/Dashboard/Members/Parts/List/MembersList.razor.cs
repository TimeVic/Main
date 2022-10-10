using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.WorkspaceMemberships;

namespace TimeTracker.Web.Pages.Dashboard.Members.Parts.List
{
    public partial class MembersList
    {
        [Inject] 
        private IState<WorkspaceMembershipsState> _state { get; set; }
    
        private RadzenDataGrid<WorkspaceMembershipDto> _grid;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            Dispatcher.Dispatch(new LoadListAction(true));
        }

        private async Task OnDeleteItemAsync(WorkspaceMembershipDto item)
        {
            var isOk = await DialogService.Confirm(
                "Are you sure you want to remove this item?",
                "Delete confirmation",
                new ConfirmOptions()
                {
                    OkButtonText = "Delete",
                    CancelButtonText = "Cancel"
                }
            );
            if (isOk.HasValue && isOk.Value)
            {
                Dispatcher.Dispatch(new DeleteMemberAction(item));
            }
            await Task.CompletedTask;
        }
        
        private async Task EditRow(WorkspaceMembershipDto item)
        {
            await _grid.EditRow(item);
        }

        private async Task OnClickSaveRow(WorkspaceMembershipDto item)
        {
            await _grid.UpdateRow(item);
        }

        private void OnClickCancelEditMode(WorkspaceMembershipDto item)
        {
            // Dispatcher.Dispatch(new RemoveEmptyClientListItemAction());
            _grid.CancelEditRow(item);
        }
    
        private async Task OnUpdateRow(WorkspaceMembershipDto item)
        {
            if (item.Id > 0)
            {
                // await UpdateApplication(item);
                return;
            }

            // Dispatcher.Dispatch(new SaveEmptyClientListItemAction());
        }
        
        private async Task ShowAddModal()
        {
            await DialogService.OpenAsync<AddMemberForm>(
                "Add new member",
                options: new DialogOptions { Width = "400px", Height = "300px", Resizable = true, Draggable = false }
            );
        }
    }
}
