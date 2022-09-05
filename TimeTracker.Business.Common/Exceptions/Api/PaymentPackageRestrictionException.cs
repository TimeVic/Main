using Domain.Abstractions;

namespace TimeTracker.Business.Common.Exceptions.Api;

public class PaymentPackageRestrictionException : Exception, IDomainException
{
    public PaymentPackageRestrictionException(string message = "Payment package restriction") : base(message)
    {
    }
}
