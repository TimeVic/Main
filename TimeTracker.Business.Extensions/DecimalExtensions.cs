using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Business.Extensions;

public static class DecimalExtensions
{
    public static string ToMoneyFormat(this decimal decimalValue)  
    {
        return decimalValue.ToString("0.00");
    }
}
