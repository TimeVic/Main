select 
    coalesce(count(distinct te.user_id), 0) as "PendingApprovalsCount"
from time_entries te
inner join workspace_members wm on wm.user_id = te.user_id and wm.workspace_id = te.workspace_id
where te.workspace_id = :workspaceId
  and te.is_marked_to_delete = false
  and te.end_time is not null
  and te.status = :statusPending
  and wm.membership_access_type_id != :ownerAccessType;
