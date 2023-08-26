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
using LoadListAction = TimeTracker.Web.Store.WorkspaceMemberships.LoadListAction;

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
        
        private void OnProjectsChanged(IEnumerable<ProjectDto> projects)
        {
            _selectedProjects = projects;
        }

        private async Task OnAdd()
        {
            await ModalDialogProviderService.ShowAddWorkspaceMembershipModal();
        }

        private async Task OnEdit(WorkspaceMembershipDto item)
        {
            await ModalDialogProviderService.ShowUpdateWorkspaceMembershipModal(item);
        }

        private async Task OnDelete(WorkspaceMembershipDto item)
        {
            var isOk = await ModalDialogProviderService.ShowDeleteConfirmationDialog();
            if (isOk.HasValue && isOk.Value)
            {
                Dispatcher.Dispatch(new DeleteMemberAction(item));
            }
            await Task.CompletedTask;
        }
    }
}
