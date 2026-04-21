select
    te.project_id as ProjectId,
    p.name as ProjectName,
    sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch,
    sum(
        case when te.user_id = :userId
                 then round(
                te.hourly_rate / 60 / 60 -- Price per second 
                    *
                extract(epoch from te.end_time - te.start_time), -- Total seconds
                2
                      )
             else 0
            end
    ) as AmountOriginal
from time_entries te
         left join projects p on te.project_id = p.id
where te.project_id in (:projectIds)
  and te.end_time is not null
  and cast(te.start_time as date) >= cast(:startDate as date)
  and cast(te.start_time as date) <= cast(:endDate as date)
group by te.project_id, p.name
