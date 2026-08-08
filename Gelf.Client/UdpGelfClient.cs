using System.IO.Compression;
using System.Net.Sockets;
using System.Text;

namespace Gelf.Client;

public sealed class UdpGelfClient : IGelfClient, IDisposable
{
    private const int MaxChunks = 128;
    private const int MaxChunkSize = 8_192;
    private const int MessageHeaderSize = 12;
    private const int MessageIdSize = 8;
    private const int MaxMessageBodySize = MaxChunkSize - MessageHeaderSize;

    private readonly UdpClient _client = new();
    private readonly MessageConverter _converter = new();

    public bool NeedCompression { get; set; } = true;

    public int CompressionThreshold { get; set; } = 512;

    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 12201;

    public UdpGelfClient()
    {
    }

    public UdpGelfClient(string host, int port)
    {
        Host = host;
        Port = port;
    }

    public UdpGelfClient(ClientSettings settings)
    {
        NeedCompression = settings.NeedCompression;
        CompressionThreshold = settings.CompressionThreshold;

        if (!string.IsNullOrWhiteSpace(settings.Host))
        {
            Host = settings.Host;
        }

        if (settings.Port != 0)
        {
            Port = settings.Port;
        }
    }

    public async Task<bool> Send(Message message)
    {
        if (string.IsNullOrWhiteSpace(message.ShortMessage))
        {
            throw new ArgumentException("Message can't be empty");
        }

        if (string.IsNullOrWhiteSpace(message.Host))
        {
            message.Host = Environment.MachineName;
        }

        var messageBytes = Encoding.UTF8.GetBytes(await _converter.ToJson(message));
        if (NeedCompression && messageBytes.Length > CompressionThreshold)
        {
            messageBytes = await CompressMessage(messageBytes);
        }

        foreach (var chunk in CreateChunks(messageBytes))
        {
            await _client.SendAsync(chunk, chunk.Length, Host, Port);
        }

        return true;
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    private static IEnumerable<byte[]> CreateChunks(byte[] messageBytes)
    {
        if (messageBytes.Length < MaxChunkSize)
        {
            yield return messageBytes;
            yield break;
        }

        var chunksCount = (int)Math.Ceiling(messageBytes.Length / (double)MaxMessageBodySize);
        if (chunksCount > MaxChunks)
        {
            yield break;
        }

        var messageId = Guid.NewGuid().ToByteArray()[..MessageIdSize];
        for (var chunkIndex = 0; chunkIndex < chunksCount; chunkIndex++)
        {
            var chunkStartIndex = chunkIndex * MaxMessageBodySize;
            var chunkBodySize = Math.Min(messageBytes.Length - chunkStartIndex, MaxMessageBodySize);
            var chunk = new byte[chunkBodySize + MessageHeaderSize];

            chunk[0] = 0x1e;
            chunk[1] = 0x0f;
            Array.Copy(messageId, 0, chunk, 2, MessageIdSize);
            chunk[10] = (byte)chunkIndex;
            chunk[11] = (byte)chunksCount;
            Array.Copy(messageBytes, chunkStartIndex, chunk, MessageHeaderSize, chunkBodySize);

            yield return chunk;
        }
    }

    private static async Task<byte[]> CompressMessage(byte[] messageBytes)
    {
        using var outputStream = new MemoryStream();
        await using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
        {
            await gzipStream.WriteAsync(messageBytes);
        }

        return outputStream.ToArray();
    }
}
