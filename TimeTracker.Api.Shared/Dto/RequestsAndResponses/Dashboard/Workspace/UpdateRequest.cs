using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace
{
    public class UpdateRequest : AddRequest
    {
        [Required]
        public Guid WorkspaceId { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string CurrencyCode { get; set; }
        
        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string TimeZone { get; set; }
        
        public void Fill(WorkspaceDto workspace)
        {
            WorkspaceId = workspace.Id;
            Name = workspace.Name;
            CurrencyCode = workspace.CurrencyCode;
            TimeZone = workspace.TimeZone;
        }
    }
}
