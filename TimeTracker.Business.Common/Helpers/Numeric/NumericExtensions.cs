using System.Numerics;

namespace TimeTracker.Business.Common.Helpers.Numeric;

public class NumericConvertor
{
    
    public static string ToString<T>(T value)
        where T : INumber<T>
        => value.ToString();

    public static T FromString<T>(string s)
        where T : INumber<T>
        => T.Parse(s, null);

    public static bool TryFromString<T>(string s, out T result)
        where T : INumber<T>
        => T.TryParse(s, null, out result);

}
