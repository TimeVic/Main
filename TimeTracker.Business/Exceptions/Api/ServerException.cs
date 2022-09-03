﻿using Domain.Abstractions;

namespace TimeTracker.Business.Exceptions.Api
{
    public class ServerException : Exception, IDomainException
    {
        public ServerException(string message = "Server error") : base(message)
        {
        }
    }
}
