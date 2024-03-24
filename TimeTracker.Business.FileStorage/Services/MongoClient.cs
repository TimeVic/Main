using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Extensions;

namespace TimeTracker.Business.FileStorage.Services;

public class MongoClient: IMongoClient
{
    private const string KEY_FILE_NAME = "fileName";
    private const string KEY_FILE_DIRECTORY = "fileDirectory";
    private const string KEY_FILE_MIME_TYPE = "mimeType";
    
    private readonly IConfiguration _configuration;
    private readonly MongoDB.Driver.MongoClient _mongoClient;
    private readonly IMongoDatabase _mongoDb;

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
    }

    public async Task<ObjectId> UploadFileFromStream(
        string bucketName,
        string directory,
        string fileName,
        Stream fileStream
    )
    {
        directory = PrepareDirectory(directory);
        var mongoBucket = GetMongoBucket(bucketName);
        return await mongoBucket.UploadFromStreamAsync(
            PrepareFileName(directory, fileName),
            fileStream,
            new GridFSUploadOptions()
            {
                Metadata = new BsonDocument()
                {
                    new BsonElement(KEY_FILE_NAME, fileName),
                    new BsonElement(KEY_FILE_DIRECTORY, directory),
                    new BsonElement(KEY_FILE_MIME_TYPE, MimeTypeHelper.GetMimeTypeByName(fileName)),
                }
            }
        );
    }
    
    public async Task<Stream> DownloadToStream(
        string bucketName,
        string directory,
        string fileName
    )
    {
        var mongoBucket = GetMongoBucket(bucketName);
        directory = PrepareDirectory(directory);
        var stream = new MemoryStream();
        await mongoBucket.DownloadToStreamByNameAsync(
            PrepareFileName(directory, fileName), 
            stream
        );
        return stream;
    }

    private string PrepareFileName(string directory, string fileName)
    {
        return $"{directory}{fileName}";
    }
    
    private string PrepareDirectory(string directory)
    {
        directory = directory.RemoveLeadingSlash().RemoveTrailingSlash();
        return $"{directory}/";
    }
    
    private IGridFSBucket GetMongoBucket(string bucketName)
    {
        return new GridFSBucket(_mongoDb, new GridFSBucketOptions()
        {
            BucketName = bucketName
        });
    }
}
