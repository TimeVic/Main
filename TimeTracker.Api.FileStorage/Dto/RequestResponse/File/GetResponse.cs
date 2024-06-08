using Api.Requests.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace TimeTracker.Api.FileStorage.Dto.RequestResponse.File;

public class GetResponse: FileStreamResult, IResponse 
{
    public GetResponse(Stream fileStream, string contentType) : base(fileStream, contentType)
    {
    }

    public GetResponse(Stream fileStream, MediaTypeHeaderValue contentType) : base(fileStream, contentType)
    {
    }
}
