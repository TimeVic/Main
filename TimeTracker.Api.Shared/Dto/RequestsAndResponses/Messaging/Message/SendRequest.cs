using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Message
{
    public class SendRequest : IRequest
    {
        [Required]
        [StringLength(11000, MinimumLength = 1)]
        public required string Text { get; set; }
        
        public Guid? ReceiverId { get; set; }
        
        public Guid? ChannelId { get; set; }
    }
}
