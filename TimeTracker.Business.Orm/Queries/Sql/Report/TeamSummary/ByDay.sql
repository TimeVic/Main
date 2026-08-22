select
    cast(x.day as timestamp) as Date,
    sum(extract(epoch from x.duration)) as DurationAsEpoch,
    sum(fn_calculate_amount(x.duration, te.hourly_rate, te.is_billable)) as ClientBillableOriginal,
    sum(fn_calculate_amount(x.duration, coalesce(wmpa.hourly_rate, 0), true)) as TeamLaborCostOriginal
from time_entries te
         inner join workspaces w on w.id = te.workspace_id
         inner join workspace_members wm on wm.user_id = te.user_id and wm.workspace_id = te.workspace_id
         left join workspace_member_project_accesses wmpa on wmpa.workspace_member_id = wm.id and wmpa.project_id = te.project_id
         cross join lateral fn_split_time_entry_by_day(te.start_time, te.end_time, w.time_zone) as x
where te.workspace_id = :workspaceId
  and te.end_time is not null
  and x.day >= cast(:startDate as date)
  and x.day <= cast(:endDate as date)
group by x.day
order by x.day;
