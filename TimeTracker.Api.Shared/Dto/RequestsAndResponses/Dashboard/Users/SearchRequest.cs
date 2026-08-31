using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;

public class SearchRequest : IRequest<SearchResponse>
{
    [Required]
    public string Query { get; set; } = string.Empty;

    public int Take { get; set; } = 10;
}
