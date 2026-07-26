select
    cast(x.day as timestamp) as date,
            sum(extract(epoch from x.duration)) as durationasepoch,
            sum(
                round(
                    (extract(epoch from x.duration) / 3600.0) * te.hourly_rate,
                    2
                )
            ) as amountoriginal
    from (
        select *
        from time_entries
        where end_time is not null
    ) te
    join workspaces w on w.id = te.workspace_id
    left join projects p on p.id = te.project_id
    cross join lateral fn_split_time_entry_by_day(
    te.start_time,
    te.end_time,
    w.time_zone
    ) as x
where te.workspace_id = :workspaceId
  and p.deleted_at is null
  and x.day >= cast(:startDate as date)
  and x.day <= cast(:endDate as date)
group by x.day
order by x.day desc
    limit 60;
