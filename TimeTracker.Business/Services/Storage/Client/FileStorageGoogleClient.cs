using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1.Data;
using Google.Apis.Upload;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Storage.Client;

public class FileStorageGoogleClient: IFileStorageGoogleClient
{
    private readonly ILogger<FileStorageGoogleClient> _logger;

    private const string CredentialsFilepath = "../../../../.credentials/google.json";
    
    private readonly GoogleCredential _credentials;
    
    private readonly string? _bucketName;
    private StorageClient _googleClient => StorageClient.Create(_credentials);
    
    public FileStorageGoogleClient(
        IConfiguration configuration,
        ILogger<FileStorageGoogleClient> logger
    )
    {
        _logger = logger;

        var filePath = Path.Combine(AssemblyUtils.GetAssemblyPath(), CredentialsFilepath);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Google Cloud credentials file not found: {filePath}");
        }

        using var credentialsStream = new FileStream(
            Path.Combine(AssemblyUtils.GetAssemblyPath(), CredentialsFilepath),
            FileMode.Open,
            FileAccess.Read
        );
        
        _credentials = CredentialFactory.FromStream<ServiceAccountCredential>(credentialsStream).ToGoogleCredential();
        if (_credentials == null)
            throw new ArgumentNullException(nameof(_credentials));
        _bucketName = configuration.GetValue<string>("Google:Storage:BucketName");
        if (_bucketName == null)
            throw new ArgumentNullException(nameof(_bucketName));
        
    }
    
    public async Task<UploadedFileDto?> Upload(
        string filePath,
        Stream fileStream,
        CancellationToken cancellationToken = default
    )
    {
        var fileExtension = Path.GetExtension(filePath).Replace(".", "");
        var mimeType = MimeTypeHelper.GetMimeTypeByExtension(fileExtension);
        
        var cloudFile = await _googleClient.UploadObjectAsync(
            _bucketName,
            filePath,
            mimeType,
            fileStream,
            cancellationToken: cancellationToken,
            options: new UploadObjectOptions()
            {
                ChunkSize = 1 * 1024 * 1024
            },
            progress: new Progress<IUploadProgress>(handler =>
            {
                var bytesString = StringUtils.BytesToString(handler.BytesSent);
                _logger.LogDebug($"GCloud file uploading. Status: {handler.Status} Uploaded: {bytesString}");
            })
        );
        if (cloudFile == null)
        {
            throw new Exception($"File was not uploaded to cloud: {filePath}");
        }
        return new UploadedFileDto()
        {

        };
    }
    
    public async Task<Stream> GetAsStream(string filePath, CancellationToken cancellationToken = default)
    {
        var fileStream = new MemoryStream();
        await _googleClient.DownloadObjectAsync(_bucketName, filePath, fileStream, cancellationToken: cancellationToken);
        return fileStream;
    }
    
    public async Task Delete(string filePath, CancellationToken cancellationToken = default)
    {
        await _googleClient.DeleteObjectAsync(_bucketName, filePath, cancellationToken: cancellationToken);
    }
}
