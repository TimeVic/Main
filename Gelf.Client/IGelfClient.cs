namespace Gelf.Client;

public interface IGelfClient
{
    Task<bool> Send(Message message);
}
