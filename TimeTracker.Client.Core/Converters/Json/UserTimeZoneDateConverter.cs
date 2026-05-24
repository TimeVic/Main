using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TimeTracker.Api.Shared.Common.Attributes;

namespace TimeTracker.Client.Core.Converters.Json;

public class UserTimeZoneDateConverter: JsonConverter<DateTime>
{
    /// <summary>
    /// Passthrough converter — writes/reads DateTime as-is without any timezone shift.
    /// Applied automatically by <see cref="ContractResolver"/> to properties decorated
    /// with <see cref="NonConvertibleDateTimeAttribute"/>.
    /// </summary>
    private sealed class PassThroughDateTimeConverter : JsonConverter<DateTime>
    {
        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
            => writer.WriteValue(value);

        public override DateTime ReadJson(
            JsonReader reader,
            Type objectType,
            DateTime existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        ) => reader.Value switch
        {
            DateTime dt => dt,
            string s    => DateTime.Parse(s),
            _           => existingValue
        };
    }

    /// <summary>
    /// Contract resolver that replaces <see cref="UserTimeZoneDateConverter"/> with a
    /// passthrough for every DateTime property annotated with
    /// <see cref="NonConvertibleDateTimeAttribute"/>.
    ///
    /// Usage:
    /// <code>
    /// new JsonSerializerSettings
    /// {
    ///     ContractResolver = new UserTimeZoneDateConverter.ContractResolver(),
    ///     Converters = { new UserTimeZoneDateConverter(tz) }
    /// }
    /// </code>
    /// </summary>
    public sealed class ContractResolver : DefaultContractResolver
    {
        private static readonly PassThroughDateTimeConverter PassThrough = new();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if ((property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                && member.GetCustomAttribute<NonConvertibleDateTimeAttribute>() is not null)
            {
                property.Converter = PassThrough;
            }

            return property;
        }
    }

    // -------------------------------------------------------------------------

    private readonly TimeZoneInfo _userTimeZone;

    public UserTimeZoneDateConverter(TimeZoneInfo userTimeZone)
    {
        _userTimeZone = userTimeZone;
    }

    public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
    {
        var userTime = TimeZoneInfo.ConvertTimeFromUtc(value.ToUniversalTime(), _userTimeZone);
        writer.WriteValue(userTime);
    }

    public override DateTime ReadJson(
        JsonReader reader,
        Type objectType,
        DateTime existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var dt = (DateTime)reader.Value;
        // ConvertTimeToUtc throws Argument_ConvertMismatch when Kind == Utc.
        // Treat any incoming value as "user-local" (Unspecified) before converting.
        var unspecified = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, _userTimeZone);
    }
}
