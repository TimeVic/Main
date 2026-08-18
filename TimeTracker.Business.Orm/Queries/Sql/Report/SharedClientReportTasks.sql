select
    p.id as ProjectId,
    t.id as TaskId,
    t.title as TaskTitle,
    sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch
from time_entries te
         inner join tasks t on t.id = te.internal_task_id
         inner join task_lists tl on tl.id = t.task_list_id
         inner join projects p on p.id = tl.project_id
where p.client_id = :clientId
  and te.project_id = p.id
  and te.end_time is not null
  and te.is_marked_to_delete = false
group by p.id, t.id, t.title
having sum(extract(epoch from te.end_time - te.start_time)) > 0
order by DurationAsEpoch desc, t.title;
