using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TimeTracker.Business.Common.Constants.Notes;

[JsonConverter(typeof(StringEnumConverter))]
public enum NoteLinkEntityType : short
{
    Client = 1,
    Project = 2,
    Task = 3
}
