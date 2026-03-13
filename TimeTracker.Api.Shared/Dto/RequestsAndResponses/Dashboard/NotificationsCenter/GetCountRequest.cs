using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.NotificationsCenter
{
    public class GetCountRequest : IRequest<GetCountResponse>
    {
        [Required]
        public Guid WorkspaceId { get; set; }
    }
}
