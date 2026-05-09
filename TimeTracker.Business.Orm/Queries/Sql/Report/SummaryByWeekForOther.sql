select
    cast(date_trunc('week', x.day) as timestamp) as WeekStartDate,
    cast(date_trunc('week', x.day) + interval '6 days' as timestamp) as WeekEndDate,
    sum(extract(epoch from x.duration)) as DurationAsEpoch,
    sum(
        case when te.user_id = :userId
                 then round(
                (extract(epoch from x.duration) / 3600.0)
                    * te.hourly_rate,
                2
                      )
             else 0
            end
    ) as AmountOriginal
    from (
        select *
        from time_entries
        where end_time is not null
    ) te
         join projects p on p.id = te.project_id
         join workspaces w on w.id = te.workspace_id
         cross join lateral fn_split_time_entry_by_day(
        te.start_time,
            te.end_time,
        w.time_zone
        ) as x
where te.project_id in (:projectIds)
  and x.day >= cast(:startDate as date)
  and x.day <= cast(:endDate as date)
group by WeekStartDate, WeekEndDate
order by WeekStartDate desc
