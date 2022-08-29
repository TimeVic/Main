﻿using Domain.Abstractions;

namespace TimeTracker.Business.Exceptions.Common
{
    public class ValidationException : Exception, IDomainException
    {
        public ValidationException(): this("")
        {
        }

        public ValidationException(string message) : base(message)
        {
        }
    }
}
