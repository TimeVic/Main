select
    c.id as ClientId,
    c.name as ClientName,
    coalesce(sum(cp.amount) filter (where cp.project_id is not null), 0) as ProjectPaymentsOriginal,
    coalesce(sum(cp.amount) filter (where cp.project_id is null), 0) as GeneralPaymentsOriginal
from clients c
         inner join client_payments cp on cp.client_id = c.id
where c.workspace_id = :workspaceId
group by c.id, c.name
order by c.name
