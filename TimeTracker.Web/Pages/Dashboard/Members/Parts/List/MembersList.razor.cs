using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Shared.Components.Form;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.WorkspaceMemberships;

namespace TimeTracker.Web.Pages.Dashboard.Members.Parts.List
{
    public partial class MembersList
    {
        [Inject] 
        private IState<WorkspaceMembershipsState> _state { get; set; }
    
        [Inject]
        public IState<ProjectState> _projectState { get; set; }
        
        private IEnumerable<ProjectDto> _selectedProjects = new List<ProjectDto>();
        private ICollection<MembershipAccessType> _allowedAccessLevels = new List<MembershipAccessType>()
        {
            MembershipAccessType.User,
            MembershipAccessType.Manager
        };

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            Dispatcher.Dispatch(new LoadListAction(true));
        }

        private string GetProjectNames(WorkspaceMembershipDto membershipDto)
        {
            return string.Join(
                ", ",
                membershipDto.ProjectAccesses.Select(
                    item => _projectState.Value.List.FirstOrDefault(project => project.Id == item.Project.Id)
                )
                    .Where(item => item != null)
                    .Select(item => item.Name)
                    .ToList()
            );
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
        
        private async Task ShowEditModal(WorkspaceMembershipDto item)
        {
            await DialogService.OpenAsync<MemberAccessForm>(
                "Change access",
                parameters: new Dictionary<string, object>()
                {
                    { "WorkspaceMembership", item }
                },
                options: new DialogOptions
                {
                    Width = "600px",
                    Height = "400px",
                    Resizable = true, 
                    Draggable = false
                }
            );
        }

        private async Task ShowAddModal()
        {
            await DialogService.OpenAsync<AddMemberForm>(
                "Add new member",
                options: new DialogOptions { Width = "400px", Height = "300px", Resizable = true, Draggable = false }
            );
        }
        
        private void OnProjectsChanged(IEnumerable<ProjectDto> projects)
        {
            _selectedProjects = projects;
        }
    }
}
