﻿using Domain.Abstractions;

namespace TimeTracker.Business.Exceptions.Common
{
    public class TooManyRequestsException : Exception, IDomainException
    {
        public TooManyRequestsException() : this("")
        {
        }

        public TooManyRequestsException(string entityName) : base($"Too many requests: {entityName}")
        {
        }
    }
}
