using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.SubTasks
{
    public class DeleteRequest : IRequest
    {
        [Required]
        public Guid SubTaskId { get; set; }
    }
}
