using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using TimeTracker.Business.Common.Helpers;

namespace TimeTracker.Business.FileStorage.Services.Storage;

public class MongoClient: IMongoClient
{
    private const string STORAGE_BUCKET_1 = "fs_1";
    
    private const string KEY_USER_BUCKET = "bucket";
    private const string KEY_FILE_NAME = "fileName";
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

        var loginConnectionPath = "";
        if (!string.IsNullOrEmpty(mongoLogin))
        {
            loginConnectionPath = $"{mongoLogin}:{mongoPassword}@";
        }
        _mongoClient = new MongoDB.Driver.MongoClient($"mongodb://{loginConnectionPath}{mongoHost}:{mongoPort}");
        _mongoDb = _mongoClient.GetDatabase(dbName);
        _mongoFsBucket = new GridFSBucket(_mongoDb, new GridFSBucketOptions()
        {
            BucketName = STORAGE_BUCKET_1
        });
    }

    public async Task<ObjectId> UploadFileFromStream(
        string usersBucketName,
        string fileName,
        Stream fileStream
    )
    {
        return await _mongoFsBucket.UploadFromStreamAsync(
            PrepareFileName(usersBucketName, fileName),
            fileStream,
            new GridFSUploadOptions()
            {
                Metadata =
                [
                    new BsonElement(KEY_FILE_NAME, fileName),
                    new BsonElement(KEY_USER_BUCKET, usersBucketName),
                    new BsonElement(KEY_FILE_MIME_TYPE, MimeTypeHelper.GetMimeTypeByName(fileName))
                ]
            }
        );
    }
    
    public async Task Delete(string mongoObjectId)
    {
        await _mongoFsBucket.DeleteAsync(new ObjectId(mongoObjectId));
    }
    
    public async Task<bool> IsExists(
        string usersBucketName,
        string fileName
    )
    {
        var filter = Builders<GridFSFileInfo>.Filter.Eq(info => info.Filename, PrepareFileName(usersBucketName, fileName));
        return (await _mongoFsBucket.FindAsync(filter)).FirstOrDefault() != null;
    }
    
    public async Task<Stream> DownloadToStream(
        string usersBucketName,
        string fileName
    )
    {
        var stream = new MemoryStream();
        await _mongoFsBucket.DownloadToStreamByNameAsync(
            PrepareFileName(usersBucketName, fileName), 
            stream
        );
        return stream;
    }

    private string PrepareFileName(string usersBucket, string fileName)
    {
        return $"{usersBucket}/{fileName}";
    }
}
