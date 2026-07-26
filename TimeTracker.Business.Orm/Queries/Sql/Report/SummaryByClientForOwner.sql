select
    c.id as ClientId,
    c.name as ClientName,
    sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch,
    sum(
        round(
            te.hourly_rate / 60 / 60 -- Price per second 
                *
            extract(epoch from te.end_time - te.start_time), -- Total seconds
            2
        )
    ) as AmountOriginal
from time_entries te
         left join projects p on p.id = te.project_id
         left join clients c on c.id = p.client_id
where te.workspace_id = :workspaceId
  and p.deleted_at is null
  and te.end_time is not null
  and cast(te.start_time as date) >= cast(:startDate as date)
  and cast(te.start_time as date) <= cast(:endDate as date)
group by c.id, c.name
