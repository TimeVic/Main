using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using TimeTracker.Business.Common.Constants.Import;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Services.Import.TimeEntry.Model;

namespace TimeTracker.Business.Services.Import.TimeEntry.Parsers;

public class ClockifyTimeEntryCsvParser : ITimeEntryCsvParser
{
    private static readonly string[] DateTimeFormats =
    [
        "MM/dd/yyyy HH:mm:ss",
        "MM/dd/yyyy H:mm:ss",
        "MM/dd/yyyy HH:mm",
        "MM/dd/yyyy H:mm",
        "M/d/yyyy HH:mm:ss",
        "M/d/yyyy H:mm:ss",
        "M/d/yyyy HH:mm",
        "M/d/yyyy H:mm",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd H:mm:ss",
        "yyyy-MM-dd HH:mm",
        "yyyy-MM-dd H:mm",
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
        "yyyy-MM-dd h:mm tt",
        "yyyy/MM/dd HH:mm:ss",
        "yyyy/MM/dd HH:mm"
    ];

    private readonly ILogger<ClockifyTimeEntryCsvParser> _logger;

    public TimeEntryImportSourceType SourceType => TimeEntryImportSourceType.Clockify;

    public ClockifyTimeEntryCsvParser(ILogger<ClockifyTimeEntryCsvParser> logger)
    {
        _logger = logger;
    }

    public async Task<IReadOnlyList<TimeEntryImportModel>> ParseAsync(
        Stream csvStream,
        CancellationToken cancellationToken = default
    )
    {
        using var reader = new StreamReader(csvStream, leaveOpen: true);
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            PrepareHeaderForMatch = args => args.Header.Trim(),
            TrimOptions = TrimOptions.Trim,
            BadDataFound = null
        };

        using var csv = new CsvReader(reader, csvConfig);

        if (!await csv.ReadAsync())
        {
            return Array.Empty<TimeEntryImportModel>();
        }

        csv.ReadHeader();
        ValidateHeaders(csv.HeaderRecord);

        var entries = new List<TimeEntryImportModel>();

        while (await csv.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var startDateStr = GetFieldValue(csv, "Start Date");
            var startTimeStr = GetFieldValue(csv, "Start Time");
            var endDateStr = GetFieldValue(csv, "End Date");
            var endTimeStr = GetFieldValue(csv, "End Time");

            if (string.IsNullOrWhiteSpace(startDateStr) || string.IsNullOrWhiteSpace(startTimeStr))
            {
                _logger.LogWarning("Skipping Clockify CSV row due to missing Start Date or Start Time.");
                continue;
            }

            if (!TryParseDateTime(startDateStr, startTimeStr, out var startTime))
            {
                _logger.LogWarning(
                    "Skipping Clockify CSV row: unable to parse start timestamp '{StartDate} {StartTime}'.",
                    startDateStr,
                    startTimeStr
                );
                continue;
            }

            DateTime? endTime = null;
            if (!string.IsNullOrWhiteSpace(endDateStr) && !string.IsNullOrWhiteSpace(endTimeStr))
            {
                if (TryParseDateTime(endDateStr, endTimeStr, out var parsedEnd))
                {
                    if (parsedEnd < startTime)
                    {
                        _logger.LogWarning(
                            "Skipping Clockify CSV row: EndTime {EndTime} is earlier than StartTime {StartTime}.",
                            parsedEnd,
                            startTime
                        );
                        continue;
                    }
                    endTime = parsedEnd;
                }
                else
                {
                    _logger.LogWarning(
                        "Skipping Clockify CSV row: unable to parse end timestamp '{EndDate} {EndTime}'.",
                        endDateStr,
                        endTimeStr
                    );
                    continue;
                }
            }

            var projectName = GetFieldValue(csv, "Project");
            var clientName = GetFieldValue(csv, "Client");
            var description = GetFieldValue(csv, "Description");
            var task = GetFieldValue(csv, "Task");
            var tagsStr = GetFieldValue(csv, "Tags");

            var tags = !string.IsNullOrWhiteSpace(tagsStr)
                ? tagsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                : Array.Empty<string>();

            bool? isBillable = null;
            var billableStr = GetFieldValue(csv, "Billable");
            if (!string.IsNullOrWhiteSpace(billableStr))
            {
                if (bool.TryParse(billableStr, out var bVal))
                {
                    isBillable = bVal;
                }
                else if (billableStr.Equals("yes", StringComparison.OrdinalIgnoreCase))
                {
                    isBillable = true;
                }
                else if (billableStr.Equals("no", StringComparison.OrdinalIgnoreCase))
                {
                    isBillable = false;
                }
            }

            decimal? hourlyRate = null;
            var rateStr = GetFieldValue(csv, "Rate") 
                ?? GetFieldValue(csv, "Hourly Rate") 
                ?? GetFieldValue(csv, "Hourly Rate (USD)")
                ?? GetFieldValue(csv, "Rate (USD)");
            if (!string.IsNullOrWhiteSpace(rateStr) && decimal.TryParse(rateStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedRate))
            {
                hourlyRate = parsedRate;
            }

            entries.Add(new TimeEntryImportModel
            {
                ClientName = string.IsNullOrWhiteSpace(clientName) ? null : clientName.Trim(),
                ProjectName = string.IsNullOrWhiteSpace(projectName) ? null : projectName.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                TaskId = string.IsNullOrWhiteSpace(task) ? null : task.Trim(),
                StartTime = startTime,
                EndTime = endTime,
                IsBillable = isBillable,
                HourlyRate = hourlyRate,
                Tags = tags.ToList()
            });
        }

        return entries;
    }

    private static void ValidateHeaders(string[]? headerRecord)
    {
        if (headerRecord == null || headerRecord.Length == 0)
        {
            throw new IncorrectFileException("CSV file contains no headers.");
        }

        var headerSet = new HashSet<string>(headerRecord.Select(h => h.Trim()), StringComparer.OrdinalIgnoreCase);
        string[] requiredHeaders = ["Start Date", "Start Time", "End Date", "End Time"];

        var missingHeaders = requiredHeaders.Where(h => !headerSet.Contains(h)).ToList();
        if (missingHeaders.Count > 0)
        {
            throw new IncorrectFileException(
                $"CSV file does not contain required Clockify columns: {string.Join(", ", missingHeaders)}"
            );
        }
    }

    private static string? GetFieldValue(CsvReader csv, string columnName)
    {
        if (csv.TryGetField<string>(columnName, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value.Trim();
        }
        return null;
    }

    private static bool TryParseDateTime(string dateStr, string timeStr, out DateTime dateTime)
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
}
