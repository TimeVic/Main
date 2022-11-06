using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership
{
    public class UpdateRequest : IRequest<WorkspaceMembershipDto>
    {
        [Required]
        [IsPositive]
        public long MembershipId { get; set; }
        
        [Required]
        public MembershipAccessType Access { get; set; }

        [Required]
        [ValidateListModels]
        public ICollection<MembershipProjectAccessRequest> ProjectsAccess { get; set; } = new List<MembershipProjectAccessRequest>();

        public void Fill(WorkspaceMembershipDto membershipDto, ICollection<ProjectDto> projects)
        {
            MembershipId = membershipDto.Id;
            Access = membershipDto.Access;
            ProjectsAccess = projects.Select(item =>
            {
                var accessItem = membershipDto.ProjectAccesses.FirstOrDefault(
                    item2 => item2.Project.Id == item.Id
                );
                return new MembershipProjectAccessRequest()
                {
                    HourlyRate = accessItem?.HourlyRate,
                    ProjectId = item.Id,
                    HasAccess = accessItem != null
                };
            }).ToList();
        }
    }
}
