using System.ComponentModel.DataAnnotations;
using System.Drawing;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag
{
    public class AddRequest : IRequest<TagDto>
    {
        public const int NameMaxLength = 50;

        [Required]
        [StringLength(NameMaxLength, MinimumLength = 2)]
        public required string Name { get; set; }
        
        [IsColor]
        public string? Color { get; set; }
    }
}
