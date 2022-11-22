using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace
{
    public class SetRedmineSettingsRequest : IRequest<WorkspaceSettingsRedmineDto>
    {
        [Required]
        [IsPositive]
        public virtual long WorkspaceId { get; set; }
        
        [Required]
        [StringLength(200)]
        [IsUrl]
        public virtual string Url { get; set; }
    
        [Required]
        [StringLength(100)]
        public virtual string ApiKey { get; set; }
    
        [Required]
        [IsPositive]
        public virtual long RedmineUserId { get; set; }
    
        [Required]
        [IsPositive]
        public virtual long ActivityId { get; set; }

        public void Fill(WorkspaceSettingsRedmineDto dto)
        {
            Url = dto.Url;
            ApiKey = dto.ApiKey;
            RedmineUserId = dto.RedmineUserId;
            ActivityId = dto.ActivityId;
        }
    }
}
