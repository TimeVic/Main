using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User
{
    public class RefreshTokenRequest : IRequest<RefreshTokenResponseDto>
    {
        [Required]
        [StringLength(512, MinimumLength = 6)]
        public string AccessToken { get; set; } = string.Empty;
        
        [Required]
        [StringLength(512, MinimumLength = 6)]
        public string JwtToken { get; set; } = string.Empty;
    }
}
