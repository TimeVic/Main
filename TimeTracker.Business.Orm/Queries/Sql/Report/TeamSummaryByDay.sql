select
    cast(x.day as timestamp) as Date,
    sum(extract(epoch from x.duration)) as DurationAsEpoch,
    sum(case when te.is_billable and te.hourly_rate is not null then round(extract(epoch from x.duration) / 3600.0 * te.hourly_rate, 2) else 0 end) as ClientBillableOriginal,
    sum(round(extract(epoch from x.duration) / 3600.0 * coalesce(wmpa.hourly_rate, 0), 2)) as TeamLaborCostOriginal
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
