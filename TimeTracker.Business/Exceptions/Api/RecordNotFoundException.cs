﻿using Domain.Abstractions;

namespace TimeTracker.Business.Exceptions.Api
{
    public class RecordNotFoundException : Exception, IDomainException
    {
        public RecordNotFoundException(string message = "Record was not found") : base(message)
        {
        }
    }
}
