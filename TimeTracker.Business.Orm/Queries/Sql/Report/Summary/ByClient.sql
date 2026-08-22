select
    c.id as ClientId,
    c.name as ClientName,
    sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch,
    sum(fn_calculate_amount(te.start_time, te.end_time, te.hourly_rate, true)) as AmountOriginal
from time_entries te
    left join projects p on p.id = te.project_id
    left join clients c on c.id = p.client_id
where te.workspace_id = :workspaceId
  and te.user_id = :userId
  and te.end_time is not null
  and cast(te.start_time as date) >= cast(:startDate as date)
  and cast(te.start_time as date) <= cast(:endDate as date)
group by c.id, c.name;
