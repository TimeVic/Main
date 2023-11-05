namespace TimeTracker.Business.Common.Helpers.MultiThreading;

public class ThreadLocker
{
    public bool IsLocked { get; private set; } = false;
    
    public void WaitUntil()
    {
        while (IsLocked)
        {
            Thread.Sleep(50);
        }
    }
    
    public void Lock()
    {
        IsLocked = true;
    }
    
    public void Release()
    {
        IsLocked = false;
    }
}
