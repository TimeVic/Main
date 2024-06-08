using Domain.Abstractions;
using MongoDB.Bson;

namespace TimeTracker.Business.FileStorage.Services.Storage;

public interface IMongoClient: IDomainService
{
    Task<ObjectId> UploadFileFromStream(
        string usersBucketName,
        string fileName,
        Stream fileStream
    );

    Task<Stream> DownloadToStream(
        string usersBucketName,
        string fileName
    );

    Task Delete(string mongoObjectId);

    Task<bool> IsExists(
        string usersBucketName,
        string fileName
    );
}
