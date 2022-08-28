using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Business.Common.Utils;

public static class PaginationUtils
{
    public static int CalculatePage(int skip, int pageSize = GlobalConstants.ListPageSize)
    {
        var page = (int) Math.Ceiling((decimal) (skip / pageSize));
        return page == 0 ? 1 : page + 1;
    }
    
    public static int CalculateOffset(int page, int pageSize = GlobalConstants.ListPageSize)
    {
        page = page - 1;
        return (int)((page <= 0 ? 0 : page) * pageSize);
    }
}
