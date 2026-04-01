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
        public Guid CurrencyId { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 3)]
        [IsTimeZone]
        public string TimeZone { get; set; } = "UTC";
        
        public void Fill(WorkspaceDto workspace)
        {
            WorkspaceId = workspace.Id;
            Name = workspace.Name;
            CurrencyId = workspace.Currency.Id;
            TimeZone = workspace.TimeZone;
        }
    }
}
