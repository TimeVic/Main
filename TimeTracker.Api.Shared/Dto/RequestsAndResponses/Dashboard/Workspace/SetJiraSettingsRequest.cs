using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace
{
    public class SetJiraSettingsRequest : IRequest<WorkspaceSettingsJiraDto>
    {
        [Required]
        [IsPositive]
        public virtual long WorkspaceId { get; set; }
        
        [Required]
        [StringLength(255)]
        public virtual string SecurityKey { get; set; } = "";
    
        [Required]
        [StringLength(255)]
        public virtual string UserName { get; set; } = "";
    
        public void Fill(WorkspaceSettingsJiraDto dto)
        {
            SecurityKey = dto.ApiKey;
            UserName = dto.UserName;
        }
    }
}
