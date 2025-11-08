using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Storage.Client;

public interface IFileStorageClient
{
    Task<UploadedFileDto?> Upload(StoredFileEntity fileToUpload, CancellationToken cancellationToken = default);

    Task<UploadedFileDto?> Upload(
        string filePath,
        Stream fileStream,
        CancellationToken cancellationToken = default
    );

    Task<Stream> GetAsStream(string filePath, CancellationToken cancellationToken = default);

    Task Delete(string filePath, CancellationToken cancellationToken = default);
}
