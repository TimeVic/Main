select
    u.id as "UserId",
    coalesce(nullif(u.user_name, ''), u.email) as "UserName",
    u.email as "Email",
    date_trunc('week', te.start_time) as "PeriodStartDate",
    (date_trunc('week', te.start_time) + interval '6 days 23 hours 59 minutes 59 seconds') as "PeriodEndDate",
    extract(epoch from sum(te.end_time - te.start_time)) as "TotalDurationSeconds",
    coalesce(sum(fn_calculate_amount(te.start_time, te.end_time, coalesce(wmpa.hourly_rate, 0), true)), 0) as "TotalDeveloperAmount",
    coalesce(sum(fn_calculate_amount(te.start_time, te.end_time, te.hourly_rate, te.is_billable)), 0) as "TotalClientAmount",
    coalesce(count(case when te.status = :statusPending then 1 end), 0) as "PendingCount"
from time_entries te
inner join users u on u.id = te.user_id
inner join workspace_members wm on wm.user_id = te.user_id and wm.workspace_id = te.workspace_id
left join workspace_member_project_accesses wmpa on wmpa.workspace_member_id = wm.id and wmpa.project_id = te.project_id
where te.workspace_id = :workspaceId
  and te.is_marked_to_delete = false
  and te.end_time is not null
  and te.status = :statusPending
  and wm.membership_access_type_id != :ownerAccessType
group by u.id, u.user_name, u.email, date_trunc('week', te.start_time)
having count(case when te.status = :statusPending then 1 end) > 0
order by 
  date_trunc('week', te.start_time) desc,
  u.user_name asc;
