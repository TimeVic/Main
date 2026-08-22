select
    p.client_id as ClientId,
    p.id as ProjectId,
    p.name as ProjectName,
    sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch,
    sum(fn_calculate_amount(te.start_time, te.end_time, te.hourly_rate, te.is_billable)) as EarnedAmountOriginal
from time_entries te
         inner join projects p on p.id = te.project_id
where te.workspace_id = :workspaceId
  and te.end_time is not null
  and te.is_billable = true
  and te.hourly_rate is not null
  and p.client_id is not null
  and te.status = 3
group by p.client_id, p.id, p.name
order by p.name
