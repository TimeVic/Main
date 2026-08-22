select
    c.id as ClientId,
    c.name as ClientName,
    p.id as ProjectId,
    p.name as ProjectName,
    sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch,
    sum(fn_calculate_amount(te.start_time, te.end_time, te.hourly_rate, te.is_billable)) as EarnedOriginal
from time_entries te
         inner join projects p on p.id = te.project_id
         inner join clients c on c.id = p.client_id
where te.workspace_id = :workspaceId
  and te.user_id = :userId
  and te.end_time is not null
  and te.end_time <= :endDate
  and te.is_billable = true
  and te.hourly_rate is not null
  and te.status = 3
group by c.id, c.name, p.id, p.name
order by c.name, p.name
