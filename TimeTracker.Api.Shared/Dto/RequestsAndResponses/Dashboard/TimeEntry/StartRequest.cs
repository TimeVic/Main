using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry
{
    public class StartRequest : IRequest<TimeEntryDto>
    {
        [Required]
        [IsPositive]
        public long WorkspaceId { get; set; }
        
        [IsPositive]
        public long? ProjectId { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; } 
        
        [StringLength(100)]
        public string? TaskId { get; set; }
        
        public bool? IsBillable { get; set; }
        
        [IsPositive]
        public decimal? HourlyRate { get; set; } 
        
        /**
         * Date of time entry
         * !Important. Should be represented in UTC timezone
         */
        [Required]
        [IsFutureOrNowDate]
        public DateTime Date { get; set; }
        
        [Required]
        [IsCorrectTimeEntryTime]
        public TimeSpan StartTime { get; set; }
    }
}
