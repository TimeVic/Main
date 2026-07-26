select
    te.user_id as UserId,
    u.user_name as UserName,
    u.email as Email,
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
         inner join users u on u.id = te.user_id
         inner join projects p on p.id = te.project_id
where te.project_id in (:projectIds)
  and p.deleted_at is null
  and te.end_time is not null
  and cast(te.start_time as date) >= cast(:startDate as date)
  and cast(te.start_time as date) <= cast(:endDate as date)
group by te.user_id, u.user_name, u.email
