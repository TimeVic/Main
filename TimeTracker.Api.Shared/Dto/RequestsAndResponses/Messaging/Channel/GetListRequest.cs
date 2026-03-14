using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Channel
{
    public class GetListRequest : IRequest<GetListResponse>
    {
        [Required]
        public Guid WorkspaceId { get; set; }
    }
}
