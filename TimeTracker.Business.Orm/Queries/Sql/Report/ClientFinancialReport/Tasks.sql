select
    t.id as Id,
    p.id as ProjectId,
    t.title as Title,
    tracked.duration_as_epoch as DurationAsEpoch
from (
         select
             te.internal_task_id as task_id,
             sum(extract(epoch from (te.end_time - te.start_time))) as duration_as_epoch
         from time_entries te
         where te.project_id = :projectId
           and te.internal_task_id is not null
           and te.end_time is not null
           and te.is_marked_to_delete = false
           and te.status = 3
         group by te.internal_task_id
     ) tracked
         inner join tasks t on t.id = tracked.task_id
         inner join task_lists tl on tl.id = t.task_list_id
         inner join projects p on p.id = tl.project_id
where t.is_archived = false
  and p.client_id = :clientId
  and p.id = :projectId
order by tracked.duration_as_epoch desc, t.title asc



