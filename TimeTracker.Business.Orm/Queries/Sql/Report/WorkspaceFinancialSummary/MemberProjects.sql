select
    wm.id as MemberId,
    p.id as ProjectId,
    p.name as ProjectName,
    c.id as ClientId,
    c.name as ClientName,
    sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch,
    sum(round(coalesce(wmpa.hourly_rate, 0) / 60.0 / 60.0 * extract(epoch from te.end_time - te.start_time), 2)) as CostAmountOriginal
from time_entries te
         inner join workspace_members wm on wm.user_id = te.user_id and wm.workspace_id = te.workspace_id
         inner join projects p on p.id = te.project_id
         left join clients c on c.id = p.client_id
         left join workspace_member_project_accesses wmpa on wmpa.workspace_member_id = wm.id and wmpa.project_id = p.id
where te.workspace_id = :workspaceId
  and te.end_time is not null
  and te.is_billable = true
  and te.hourly_rate is not null
group by wm.id, p.id, p.name, c.id, c.name
order by p.name
