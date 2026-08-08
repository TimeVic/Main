using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Gelf.Client;

public sealed class MessageConverter
{
    private static readonly JsonSerializer Serializer = new();

    public async Task<string> ToJson(Message message)
    {
        var stringBuilder = new System.Text.StringBuilder();
        using var stringWriter = new StringWriter(stringBuilder);
        using var writer = new JsonTextWriter(stringWriter)
        {
            Formatting = Formatting.Indented
        };

        await writer.WriteStartObjectAsync();
        await WriteObjectUnlessNull(writer, "version", message.Version);
        await WriteObjectUnlessNull(writer, "host", message.Host);
        await WriteObjectUnlessNull(writer, "short_message", $"[{message.Level.ToString().ToUpperInvariant()}] {message.ShortMessage}");
        await WriteObjectUnlessNull(writer, "level", (int)message.Level);

        foreach (var field in message.AdditionalFields)
        {
            await WriteAdditionalField(writer, field);
        }

        await writer.WriteEndObjectAsync();
        return stringBuilder.ToString();
    }

    private static async Task WriteObjectUnlessNull(JsonWriter writer, string key, object? value)
    {
        if (value is null)
        {
            return;
        }

        await writer.WritePropertyNameAsync(key);
        await writer.WriteValueAsync(value);
    }

    private static async Task WriteAdditionalField(JsonWriter writer, KeyValuePair<string, object?> field)
    {
        if (field.Value is null)
        {
            return;
        }

        if (!Regex.IsMatch(field.Key, @"^[\w\.\-]*$") || field.Key == "id")
        {
            throw new ArgumentException($"Incorrect format of additional field key: {field.Key}");
        }

        await writer.WritePropertyNameAsync($"_{field.Key}");
        switch (field.Value)
        {
            case string value:
                await writer.WriteValueAsync(value);
                return;
            case int value:
                await writer.WriteValueAsync(value);
                return;
            case double value:
                await writer.WriteValueAsync(value);
                return;
            case IEnumerable<string> value:
                await WriteArray(writer, value);
                return;
            case IEnumerable<int> value:
                await WriteArray(writer, value);
                return;
            case IEnumerable<double> value:
                await WriteArray(writer, value);
                return;
            default:
                Serializer.Serialize(writer, field.Value, field.Value.GetType());
                return;
        }
    }

    private static async Task WriteArray<T>(JsonWriter writer, IEnumerable<T> values)
    {
        await writer.WriteStartArrayAsync();
        foreach (var value in values)
        {
            await writer.WriteValueAsync(value);
        }

        await writer.WriteEndArrayAsync();
    }
}
