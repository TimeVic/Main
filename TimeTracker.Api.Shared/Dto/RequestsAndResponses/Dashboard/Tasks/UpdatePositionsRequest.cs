using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks
{
    public class UpdatePositionsRequest : IRequest
    {   
        [Required]
        public Guid TaskListId { get; set; }
        
        public IDictionary<Guid, int> Items { get; set; } = new Dictionary<Guid, int>();
    }
}
