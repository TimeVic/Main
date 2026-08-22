select
    cast(extract(month from x.day) as int) as Month,
    cast(extract(year from x.day) as int) as Year,
    sum(extract(epoch from x.duration)) as DurationAsEpoch,
    sum(fn_calculate_amount(x.duration, te.hourly_rate, true)) as AmountOriginal
from time_entries te
    join workspaces w on w.id = te.workspace_id
    cross join lateral fn_split_time_entry_by_day(te.start_time, te.end_time, w.time_zone) as x
where te.workspace_id = :workspaceId
  and te.user_id = :userId
  and te.end_time is not null
  and x.day >= cast(:startDate as date)
  and x.day <= cast(:endDate as date)
group by year, month
order by year desc, month desc;
