using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Extensions;

namespace TimeTracker.Business.FileStorage.Services;

public class MongoClient: IMongoClient
{
    private const string STORAGE_BUCKET_1 = "fs_1";
    
    private const string KEY_USER_BUCKET = "bucket";
    private const string KEY_FILE_NAME = "fileName";
    private const string KEY_FILE_DIRECTORY = "fileDirectory";
    private const string KEY_FILE_MIME_TYPE = "mimeType";
    
    private readonly IConfiguration _configuration;
    private readonly MongoDB.Driver.MongoClient _mongoClient;
    private readonly IMongoDatabase _mongoDb;
    private readonly GridFSBucket _mongoFsBucket;

    public MongoClient(IConfiguration configuration)
    {
        _configuration = configuration;
        
        var mongoHost = _configuration.GetValue<string>("Mongo:Host");
        var mongoPort = _configuration.GetValue<int>("Mongo:Port");
        var mongoLogin = _configuration.GetValue<string>("Mongo:Login");
        var mongoPassword = _configuration.GetValue<string>("Mongo:Password");
        var dbName = _configuration.GetValue<string>("Mongo:DbName");
        
        _mongoClient = new MongoDB.Driver.MongoClient($"mongodb://{mongoLogin}:{mongoPassword}@{mongoHost}:{mongoPort}");
        _mongoDb = _mongoClient.GetDatabase(dbName);
        _mongoFsBucket = new GridFSBucket(_mongoDb, new GridFSBucketOptions()
        {
            BucketName = STORAGE_BUCKET_1
        });
    }

    public async Task<ObjectId> UploadFileFromStream(
        string usersBucketName,
        string directory,
        string fileName,
        Stream fileStream
    )
    {
        directory = PrepareFileDirectory(directory);
        return await _mongoFsBucket.UploadFromStreamAsync(
            PrepareFileName(usersBucketName, directory, fileName),
            fileStream,
            new GridFSUploadOptions()
            {
                Metadata = new BsonDocument()
                {
                    new BsonElement(KEY_FILE_NAME, fileName),
                    new BsonElement(KEY_FILE_DIRECTORY, directory),
                    new BsonElement(KEY_USER_BUCKET, usersBucketName),
                    new BsonElement(KEY_FILE_MIME_TYPE, MimeTypeHelper.GetMimeTypeByName(fileName)),
                }
            }
        );
    }
    
    public async Task<Stream> DownloadToStream(
        string usersBucketName,
        string directory,
        string fileName
    )
    {
        directory = PrepareFileDirectory(directory);
        var stream = new MemoryStream();
        await _mongoFsBucket.DownloadToStreamByNameAsync(
            PrepareFileName(usersBucketName, directory, fileName), 
            stream
        );
        return stream;
    }

    private string PrepareFileName(string usersBucket, string directory, string fileName)
    {
        return $"{usersBucket}/{directory}{fileName}";
    }
    
    private string PrepareFileDirectory(string directory)
    {
        directory = directory.Trim().RemoveLeadingSlash().RemoveTrailingSlash();
        if (!string.IsNullOrEmpty(directory))
            directory += "/";
        return directory;
    }
}
