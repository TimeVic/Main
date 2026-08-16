select
    u.user_name as UserName,
    u.email as Email,
    sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch,
    sum(case when te.is_billable and te.hourly_rate is not null then round(extract(epoch from te.end_time - te.start_time) / 3600.0 * te.hourly_rate, 2) else 0 end) as ClientBillableOriginal,
    sum(round(extract(epoch from te.end_time - te.start_time) / 3600.0 * coalesce(wmpa.hourly_rate, 0), 2)) as TeamLaborCostOriginal
from time_entries te
         inner join workspace_members wm on wm.user_id = te.user_id and wm.workspace_id = te.workspace_id
         inner join users u on u.id = te.user_id
         left join workspace_member_project_accesses wmpa on wmpa.workspace_member_id = wm.id and wmpa.project_id = te.project_id
where te.workspace_id = :workspaceId
  and te.end_time is not null
  and te.start_time >= :startDate
  and te.start_time <= :endDate
group by u.id, u.user_name, u.email
order by u.user_name, u.email;
