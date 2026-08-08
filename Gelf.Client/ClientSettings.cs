namespace Gelf.Client;

public sealed class ClientSettings
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public bool NeedCompression { get; set; }

    public int CompressionThreshold { get; set; }
}
