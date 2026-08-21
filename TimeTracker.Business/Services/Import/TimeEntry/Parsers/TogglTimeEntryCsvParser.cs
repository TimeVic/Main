using CsvHelper;
using Microsoft.Extensions.Logging;
using TimeTracker.Business.Common.Constants.Import;
using TimeTracker.Business.Services.Import.TimeEntry.Model;

namespace TimeTracker.Business.Services.Import.TimeEntry.Parsers;

public class TogglTimeEntryCsvParser : BaseTimeEntryCsvParser
{
    private static readonly string[] RequiredHeaders = ["Start date", "Start time", "End date", "End time"];

    private readonly ILogger<TogglTimeEntryCsvParser> _logger;

    public override TimeEntryImportSourceType SourceType => TimeEntryImportSourceType.Toggl;

    public TogglTimeEntryCsvParser(ILogger<TogglTimeEntryCsvParser> logger)
    {
        _logger = logger;
    }

    public override async Task<IReadOnlyList<TimeEntryImportModel>> ParseAsync(
        Stream csvStream,
        CancellationToken cancellationToken = default
    )
    {
        using var reader = new StreamReader(csvStream, leaveOpen: true);
        using var csv = new CsvReader(reader, CreateCsvConfiguration());

        if (!await csv.ReadAsync())
        {
            return Array.Empty<TimeEntryImportModel>();
        }

        csv.ReadHeader();
        ValidateRequiredHeaders(csv.HeaderRecord, RequiredHeaders, "Toggl");

        var entries = new List<TimeEntryImportModel>();

        while (await csv.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var startDateStr = GetFieldValue(csv, "Start date") ?? GetFieldValue(csv, "Start Date");
            var startTimeStr = GetFieldValue(csv, "Start time") ?? GetFieldValue(csv, "Start Time");
            var endDateStr = GetFieldValue(csv, "End date") ?? GetFieldValue(csv, "End Date");
            var endTimeStr = GetFieldValue(csv, "End time") ?? GetFieldValue(csv, "End Time");

            if (string.IsNullOrWhiteSpace(startDateStr) || string.IsNullOrWhiteSpace(startTimeStr))
            {
                _logger.LogWarning("Skipping Toggl CSV row due to missing Start date or Start time.");
                continue;
            }

            if (!TryParseDateTime(startDateStr, startTimeStr, out var startTime))
            {
                _logger.LogWarning(
                    "Skipping Toggl CSV row: unable to parse start timestamp '{StartDate} {StartTime}'.",
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
                            "Skipping Toggl CSV row: EndTime {EndTime} is earlier than StartTime {StartTime}.",
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
                        "Skipping Toggl CSV row: unable to parse end timestamp '{EndDate} {EndTime}'.",
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

            var isBillable = ParseIsBillable(GetFieldValue(csv, "Billable"));
            var hourlyRate = ParseHourlyRate(
                GetFieldValue(csv, "Rate"),
                GetFieldValue(csv, "Hourly rate"),
                GetFieldValue(csv, "Hourly Rate"),
                GetFieldValue(csv, "Hourly Rate (USD)"),
                GetFieldValue(csv, "Rate (USD)"),
                GetFieldValue(csv, "Amount (USD)"),
                GetFieldValue(csv, "Amount")
            );

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
}
