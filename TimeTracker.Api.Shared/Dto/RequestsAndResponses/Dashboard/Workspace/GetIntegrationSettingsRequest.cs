using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace
{
    public class GetIntegrationSettingsRequest : IRequest<GetIntegrationSettingsResponse>
    {
        [Required]
        [IsPositive]
        public virtual long WorkspaceId { get; set; }
    }
}
