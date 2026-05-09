using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace
{
    public class SetJiraSettingsRequest : IRequest<WorkspaceSettingsJiraDto>
    {
        [Required]
        [Url]
        [StringLength(255)]
        public virtual string? Url { get; set; }
        
        [Required]
        [StringLength(255)]
        public virtual string? ApiKey { get; set; }
    
        [Required]
        [StringLength(255)]
        public virtual string? UserName { get; set; }
    
        public void Fill(WorkspaceSettingsJiraDto dto)
        {
            Url = dto.Url;
            ApiKey = dto.ApiKey;
            UserName = dto.UserName;
        }
    }
}
