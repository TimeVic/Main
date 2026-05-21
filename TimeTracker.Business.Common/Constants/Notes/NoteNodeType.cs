using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TimeTracker.Business.Common.Constants.Notes;

[JsonConverter(typeof(StringEnumConverter))]
public enum NoteNodeType : short
{
    Folder = 1,
    Document = 2
}
