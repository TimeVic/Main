with filtered as (
    select te.id, te.start_time, cast(te.start_time as date) as day_key
    from time_entries te
    where te.workspace_id = :workspaceId
        and te.user_id = :userId
        and te.is_marked_to_delete = false
),
ordered_days as (
    select
        d.day_key,
        dense_rank() over (order by d.day_key desc) as day_rank
        from (
        select distinct day_key
        from filtered
    ) d
),
page_days as (
    select day_key
    from ordered_days
    where day_rank > :dayOffset
    and day_rank <= (:dayOffset + :dayPageSize)
)
select f.id as id
from filtered f
join page_days pd on pd.day_key = f.day_key
order by f.start_time desc
