﻿using Domain.Abstractions;

namespace TimeTracker.Business.Exceptions.Api
{
    public class TooManyRecordsException : Exception, IDomainException
    {
        public TooManyRecordsException(string message = "Too many records") : base(message)
        {
        }
    }
}
