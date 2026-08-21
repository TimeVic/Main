using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using TimeTracker.Business.Common.Constants.Import;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Services.Import.TimeEntry.Model;

namespace TimeTracker.Business.Services.Import.TimeEntry.Parsers;

public abstract class BaseTimeEntryCsvParser : ITimeEntryCsvParser
{
    public static readonly string[] DateTimeFormats =
    [
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd H:mm:ss",
        "yyyy-MM-dd HH:mm",
        "yyyy-MM-dd H:mm",
        "yyyy/MM/dd HH:mm:ss",
        "yyyy/MM/dd H:mm:ss",
        "yyyy/MM/dd HH:mm",
        "yyyy/MM/dd H:mm",
        "MM/dd/yyyy HH:mm:ss",
        "MM/dd/yyyy H:mm:ss",
        "MM/dd/yyyy HH:mm",
        "MM/dd/yyyy H:mm",
        "M/d/yyyy HH:mm:ss",
        "M/d/yyyy H:mm:ss",
        "M/d/yyyy HH:mm",
        "M/d/yyyy H:mm",
        "dd/MM/yyyy HH:mm:ss",
        "dd/MM/yyyy H:mm:ss",
        "dd/MM/yyyy HH:mm",
        "dd/MM/yyyy H:mm",
        "dd.MM.yyyy HH:mm:ss",
        "dd.MM.yyyy H:mm:ss",
        "dd.MM.yyyy HH:mm",
        "dd.MM.yyyy H:mm",
        "MM/dd/yyyy hh:mm:ss tt",
        "MM/dd/yyyy h:mm:ss tt",
        "MM/dd/yyyy hh:mm tt",
        "MM/dd/yyyy h:mm tt",
        "M/d/yyyy hh:mm:ss tt",
        "M/d/yyyy h:mm:ss tt",
        "M/d/yyyy hh:mm tt",
        "M/d/yyyy h:mm tt",
        "yyyy-MM-dd hh:mm:ss tt",
        "yyyy-MM-dd h:mm:ss tt",
        "yyyy-MM-dd hh:mm tt",
        "yyyy-MM-dd h:mm tt"
    ];

    public abstract TimeEntryImportSourceType SourceType { get; }

    public abstract Task<IReadOnlyList<TimeEntryImportModel>> ParseAsync(
        Stream csvStream,
        CancellationToken cancellationToken = default
    );

    protected static CsvConfiguration CreateCsvConfiguration()
    {
        return new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            PrepareHeaderForMatch = args => args.Header.Trim(),
            TrimOptions = TrimOptions.Trim,
            BadDataFound = null
        };
    }

    protected static void ValidateRequiredHeaders(string[]? headerRecord, string[] requiredHeaders, string sourceName)
    {
        if (headerRecord == null || headerRecord.Length == 0)
        {
            throw new IncorrectFileException("CSV file contains no headers.");
        }

        var headerSet = new HashSet<string>(headerRecord.Select(h => h.Trim()), StringComparer.OrdinalIgnoreCase);
        var missingHeaders = requiredHeaders.Where(h => !headerSet.Contains(h)).ToList();
        if (missingHeaders.Count > 0)
        {
            throw new IncorrectFileException(
                $"CSV file does not contain required {sourceName} columns: {string.Join(", ", missingHeaders)}"
            );
        }
    }

    protected static string? GetFieldValue(CsvReader csv, string columnName)
    {
        if (csv.TryGetField<string>(columnName, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value.Trim();
        }
        return null;
    }

    protected static bool TryParseDateTime(string dateStr, string timeStr, out DateTime dateTime)
    {
        var combined = $"{dateStr.Trim()} {timeStr.Trim()}";
        if (DateTime.TryParseExact(
            combined,
            DateTimeFormats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out dateTime
        ))
        {
            return true;
        }

        return DateTime.TryParse(combined, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
    }

    protected static bool? ParseIsBillable(string? billableStr)
    {
        if (string.IsNullOrWhiteSpace(billableStr))
        {
            return null;
        }

        if (bool.TryParse(billableStr, out var bVal))
        {
            return bVal;
        }
        if (billableStr.Equals("yes", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        if (billableStr.Equals("no", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return null;
    }

    protected static decimal? ParseHourlyRate(params string?[] rateCandidates)
    {
        foreach (var rateStr in rateCandidates)
        {
            if (!string.IsNullOrWhiteSpace(rateStr) &&
                decimal.TryParse(rateStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedRate))
            {
                return parsedRate;
            }
        }
        return null;
    }
}
