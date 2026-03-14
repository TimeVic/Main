using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Channel
{
    public class CreateRequest : IRequest
    {
        [Required]
        public Guid WorkspaceId { get; set; }
        
        [Required]
        [IsSlug]
        public required string Slug { get; set; }

        public ICollection<Guid> MemberIds { get; set; } = new List<Guid>();
    }
}
