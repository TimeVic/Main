using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks
{
    public class UpdatePositionsRequest : IRequest
    {   
        [Required]
        [IsPositive]
        public long TaskListId { get; set; }
        
        public IDictionary<long, int> Items { get; set; } = new Dictionary<long, int>();
    }
}
