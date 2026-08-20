using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Constants.Notes;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

public class GetNotesTreeRequest : IRequest<GetNotesTreeResponse>
{
    public bool IncludeArchived { get; set; }

    public NoteVisibility? Visibility { get; set; }
}
