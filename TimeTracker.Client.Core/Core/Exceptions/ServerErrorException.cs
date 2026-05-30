namespace TimeTracker.Client.Core.Core.Exceptions
{
    public class ServerErrorException: Exception
    {
        public ServerErrorException(string message = "Connection error"): base(message)
        {
            
        }
    }
}