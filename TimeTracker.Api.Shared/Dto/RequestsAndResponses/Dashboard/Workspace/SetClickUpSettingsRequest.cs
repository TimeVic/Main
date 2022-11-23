using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace
{
    public class SetClickUpSettingsRequest : IRequest<WorkspaceSettingsClickUpDto>
    {
        [Required]
        [IsPositive]
        public virtual long WorkspaceId { get; set; }
        
        [Required]
        [StringLength(255)]
        public virtual string SecurityKey { get; set; } = "";
    
        [Required]
        [StringLength(255)]
        public virtual string TeamId { get; set; } = "";
    
        [Required]
        public virtual bool IsCustomTaskIds { get; set; }
    
        public virtual bool IsFillTimeEntryWithTaskDetails { get; set; }

        public void Fill(WorkspaceSettingsClickUpDto dto)
        {
            SecurityKey = dto.SecurityKey;
            TeamId = dto.TeamId;
            IsCustomTaskIds = dto.IsCustomTaskIds;
            IsFillTimeEntryWithTaskDetails = dto.IsFillTimeEntryWithTaskDetails;
        }
    }
}
