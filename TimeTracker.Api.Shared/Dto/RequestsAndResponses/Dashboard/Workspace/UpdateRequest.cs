using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace
{
    public class UpdateRequest : AddRequest
    {
        [Required]
        [IsPositive]
        public long WorkspaceId { get; set; }

        public void Fill(WorkspaceDto workspace)
        {
            WorkspaceId = workspace.Id;
            Name = workspace.Name;
        }
    }
}
