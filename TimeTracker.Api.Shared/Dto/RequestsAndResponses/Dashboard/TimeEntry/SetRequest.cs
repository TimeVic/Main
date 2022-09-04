using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry
{
    public class SetRequest : IRequest<TimeEntryDto>
    {
        [IsPositive]
        public long? Id { get; set; }

        [Required]
        [IsPositive]
        public long WorkspaceId { get; set; }
        
        [IsPositive]
        public long? ProjectId { get; set; }
        
        public string? Description { get; set; }
    
        public decimal? HourlyRate { get; set; }
    
        public bool IsBillable { get; set; }
    
        public DateTime StartTime { get; set; }
    
        public DateTime EndTime { get; set; }
    }
}
