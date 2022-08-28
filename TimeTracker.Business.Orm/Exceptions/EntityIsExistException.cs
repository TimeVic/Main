namespace TimeTracker.Business.Orm.Exceptions
{
    public class EntityIsExistException: Exception
    {
        public EntityIsExistException(string name = "") : base($"Record is exist. {name}")
        {
        }
    }
}