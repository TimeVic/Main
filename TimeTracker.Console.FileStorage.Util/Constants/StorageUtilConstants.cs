namespace TimeTracker.Console.FileStorage.Util.Constants;

public static class StorageUtilConstants
{
    public static string UserDir => Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
    
    public static string UtilDir => Path.Combine(UserDir, ".timevic", "storage");
    
    public static string ConfigFile => Path.Combine(UtilDir, "config.json");
}
