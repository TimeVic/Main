using System.Reflection;
using Gelf.Client;
using Microsoft.Extensions.Configuration;

namespace TimeTracker.Business.Logging.Client.GrayLog;

public sealed class GraylogGelfClient : IGraylogGelfClient
{
    private readonly UdpGelfClient _client;
    private readonly bool _isGraylogEnabled;
    private readonly string? _appName;
    private readonly string _environment;

    public GraylogGelfClient(IConfiguration configuration)
    {
        var host = configuration.GetValue<string>("App:Logging:GrayLog:Host");
        _client = new UdpGelfClient(new ClientSettings
        {
            Host = host ?? string.Empty,
            Port = configuration.GetValue<int>("App:Logging:GrayLog:Port", 12201),
            NeedCompression = false
        });
        _isGraylogEnabled = !string.IsNullOrWhiteSpace(host);
        _appName = configuration.GetValue<string>("App:Name");
        _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
    }

    public void Send(
        string message,
        ICollection<KeyValuePair<string, object?>> fields,
        object? additionalDataObject = null
    )
    {
        if (!_isGraylogEnabled)
        {
            return;
        }

        var messageFields = fields.ToList();
        messageFields.Add(new KeyValuePair<string, object?>("AppName", _appName));
        messageFields.Add(new KeyValuePair<string, object?>("Environment", _environment));

        if (additionalDataObject is not null)
        {
            foreach (var property in additionalDataObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                messageFields.Add(new KeyValuePair<string, object?>(property.Name, property.GetValue(additionalDataObject)));
            }
        }

        ThreadPool.QueueUserWorkItem(async _ =>
        {
            await _client.Send(new Message
            {
                ShortMessage = message,
                Level = ELevel.Debug,
                Time = DateTime.UtcNow,
                AdditionalFields = messageFields
            });
        });
    }
}
