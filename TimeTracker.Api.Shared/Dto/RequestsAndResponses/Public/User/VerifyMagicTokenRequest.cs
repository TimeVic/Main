using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;

public class VerifyMagicTokenRequest : IRequest<LoginResponseDto>
{
    [Required]
    public string Token { get; set; } = string.Empty;
}
