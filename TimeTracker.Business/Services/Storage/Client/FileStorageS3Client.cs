using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Storage.Client;

public partial class FileStorageS3S3Client: IFileStorageS3Client
{
    private readonly ILogger<IFileStorageGoogleClient> _logger;

    private readonly string? _bucketName;
    private readonly AmazonS3Client _s3Client;
    
    public FileStorageS3S3Client(
        IConfiguration configuration,
        ILogger<IFileStorageGoogleClient> logger
    )
    {
        _logger = logger;

        var accessKey = configuration.GetValue<string>("AWS:S3:AccessKey");
        if (accessKey == null)
            throw new ArgumentNullException(nameof(accessKey));
        var secretKey = configuration.GetValue<string>("AWS:S3:SecretKey");
        if (secretKey == null)
            throw new ArgumentNullException(nameof(secretKey));
        _bucketName = configuration.GetValue<string>("AWS:S3:BucketName");
        if (_bucketName == null)
            throw new ArgumentNullException(nameof(_bucketName));

        var config = new AmazonS3Config()
        {
            RegionEndpoint = Amazon.RegionEndpoint.EUCentral1,
            DisableLogging = true,
            BufferSize = 65536, // 64KB Use a larger buffer size, normally 8K default.
            DefaultConfigurationMode = DefaultConfigurationMode.InRegion,
            UseFIPSEndpoint = false,
            ProgressUpdateInterval = 1 * 1024 * 1024
        };
        var options = new BasicAWSCredentials(accessKey, secretKey);
        _s3Client = new AmazonS3Client(options, config);
    }
    
    
    public async Task<UploadedFileDto?> Upload(StoredFileEntity fileToUpload, CancellationToken cancellationToken = default)
    {
        using var fileStream = new MemoryStream();
        fileStream.Write(fileToUpload.DataToUpload);
        return await Upload(fileToUpload.CloudFilePath, fileStream, cancellationToken);
    }
    
    public async Task<UploadedFileDto?> Upload(
        string filePath,
        Stream fileStream,
        CancellationToken cancellationToken = default
    )
    {
        var s3Request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = filePath,
            InputStream = fileStream,
            AutoCloseStream = false,
            StreamTransferProgress = (sender, args) =>
            {
                _logger.LogTrace($"S3 file uploading progress: {args.PercentDone}%");
            }
        };

        _logger.LogDebug($"S3 file uploading started: {filePath}");
        var response = await _s3Client.PutObjectAsync(s3Request, cancellationToken);
        if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
        {
            return new UploadedFileDto()
            {

            };
        }

        throw new Exception($"File uploading error via S3 client: {response.HttpStatusCode}");
    }
    
    public async Task<Stream> GetAsStream(string filePath, CancellationToken cancellationToken = default)
    {
        var response = await _s3Client.GetObjectAsync(_bucketName, filePath);
        if (response == null)
        {
            throw new RecordNotFoundException($"S3 File not found: {filePath}");
        }

        var fileStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(fileStream, cancellationToken);
        return fileStream;
    }
    
    public async Task Delete(string filePath, CancellationToken cancellationToken = default)
    {
        await _s3Client.DeleteObjectAsync(_bucketName, filePath, cancellationToken);
    }
}
