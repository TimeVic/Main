using System;

namespace AspNetCore.ApiControllers.Abstractions;

public class BadResponseModel
{
    public string Type { get; set; }
    public string Message { get; set; }

    public BadResponseModel()
    {
    }

    public BadResponseModel(Exception exception)
    {
        Type = exception.GetType().Name;
        Message = exception.Message;
    }
    
    public BadResponseModel(string message)
    {
        Type = "ServerError";
        Message = message;
    }
}
