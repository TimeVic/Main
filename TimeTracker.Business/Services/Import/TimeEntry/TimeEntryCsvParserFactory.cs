using Domain.Abstractions;
using TimeTracker.Business.Common.Constants.Import;
using TimeTracker.Business.Services.Import.TimeEntry.Parsers;

namespace TimeTracker.Business.Services.Import.TimeEntry;

public interface ITimeEntryCsvParserFactory : IDomainService
{
    ITimeEntryCsvParser GetParser(TimeEntryImportSourceType sourceType);
}

public class TimeEntryCsvParserFactory : ITimeEntryCsvParserFactory
{
    private readonly IEnumerable<ITimeEntryCsvParser> _parsers;

    public TimeEntryCsvParserFactory(IEnumerable<ITimeEntryCsvParser> parsers)
    {
        _parsers = parsers;
    }

    public ITimeEntryCsvParser GetParser(TimeEntryImportSourceType sourceType)
    {
        var parser = _parsers.FirstOrDefault(p => p.SourceType == sourceType);
        if (parser == null)
        {
            throw new NotSupportedException($"Import source '{sourceType}' is not supported.");
        }
        return parser;
    }
}
