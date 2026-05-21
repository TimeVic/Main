using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

public class GetNotesTreeRequest : IRequest<GetNotesTreeResponse>
{
    public bool IncludeArchived { get; set; }
}
