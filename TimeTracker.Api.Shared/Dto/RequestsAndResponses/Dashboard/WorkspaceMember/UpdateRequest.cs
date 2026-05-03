using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember
{
    public class UpdateRequest : IRequest<WorkspaceMemberDto>
    {
        [Required]
        public Guid MemberId { get; set; }
        
        [Required]
        public MembershipAccessType Access { get; set; }

        [Required]
        [ValidateListModels]
        public ICollection<MemberProjectAccessRequest> ProjectsAccess { get; set; } = new List<MemberProjectAccessRequest>();

        public void Fill(WorkspaceMemberDto memberDto, ICollection<ProjectDto> projects)
        {
            MemberId = memberDto.Id;
            Access = memberDto.Access;
            ProjectsAccess = projects.Select(item =>
            {
                var accessItem = memberDto.ProjectAccesses.FirstOrDefault(
                    item2 => item2.Project.Id == item.Id
                );
                return new MemberProjectAccessRequest()
                {
                    HourlyRate = accessItem?.HourlyRate,
                    ProjectId = item.Id,
                    HasAccess = accessItem != null
                };
            }).ToList();
        }
    }
}
