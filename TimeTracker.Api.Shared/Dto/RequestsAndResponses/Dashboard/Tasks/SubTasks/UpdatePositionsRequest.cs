using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.SubTasks
{
    public class UpdatePositionsRequest : IRequest
    {
        [Required]
        public Guid TaskId { get; set; }

        [Required]
        public IDictionary<Guid, int> Positions { get; set; } = new Dictionary<Guid, int>();
    }
}
