using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TimeTracker.Business.Common.Constants.Notes;

[JsonConverter(typeof(StringEnumConverter))]
public enum NoteVisibility : short
{
    Private = 1,
    Workspace = 2
}
