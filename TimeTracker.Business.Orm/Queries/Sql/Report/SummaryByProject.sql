select
    te.project_id as ProjectId,
    p.name as ProjectName,
    sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch,
    sum(round(te.hourly_rate / 60 / 60 * extract(epoch from te.end_time - te.start_time), 2)) as AmountOriginal
from time_entries te
    left join projects p on te.project_id = p.id
where te.workspace_id = :workspaceId
  and te.user_id = :userId
  and te.end_time is not null
  and cast(te.start_time as date) >= cast(:startDate as date)
  and cast(te.start_time as date) <= cast(:endDate as date)
group by te.project_id, p.name;
