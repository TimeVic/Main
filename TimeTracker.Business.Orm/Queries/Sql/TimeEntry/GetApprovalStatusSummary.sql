with filtered_entries as (
    select
        te.status,
        te.is_billable,
        te.hourly_rate,
        case 
            when te.end_time is not null then extract(epoch from (te.end_time - te.start_time))
            else 0
        end as duration_seconds
    from time_entries te
    where te.workspace_id = :workspaceId
      and te.user_id = :userId
      and te.is_marked_to_delete = false
      and te.status in (:statusDraft, :statusPending, :statusRejected)
),
latest_rejection as (
    select ter.reason
    from time_entry_rejects ter
    inner join time_entries te on te.id = ter.time_entry_id
    where te.workspace_id = :workspaceId
      and te.user_id = :userId
      and te.is_marked_to_delete = false
      and te.status = :statusRejected
    order by ter.created_at desc
    limit 1
)
select
    coalesce(count(case when fe.status = :statusDraft then 1 end), 0) as "DraftCount",
    coalesce(sum(case when fe.status = :statusDraft then fe.duration_seconds else 0 end), 0) as "DraftDurationSeconds",
    coalesce(sum(case when fe.status = :statusDraft then fn_calculate_amount(fe.duration_seconds, fe.hourly_rate, fe.is_billable) else 0 end), 0) as "DraftAmount",
    
    coalesce(count(case when fe.status = :statusPending then 1 end), 0) as "PendingCount",
    coalesce(sum(case when fe.status = :statusPending then fe.duration_seconds else 0 end), 0) as "PendingDurationSeconds",
    coalesce(sum(case when fe.status = :statusPending then fn_calculate_amount(fe.duration_seconds, fe.hourly_rate, fe.is_billable) else 0 end), 0) as "PendingAmount",
    
    coalesce(count(case when fe.status = :statusRejected then 1 end), 0) as "RejectedCount",
    (select reason from latest_rejection) as "LatestRejectionReason"
from filtered_entries fe;
