using Domain.Abstractions;

namespace TimeTracker.Business.Common.Exceptions.Common
{
    public class ItemNotFoundException : Exception, IDomainException
    {
        public ItemNotFoundException(string name = "") : base($"Item not found: {name}")
        {
        }

        public ItemNotFoundException(long id) : this(id.ToString())
        {
        }
    }
}
