select
    cp.payment_time as PaymentTime,
    cp.amount as AmountOriginal,
    p.name as ProjectName,
    cp.description as Description,
    cp.created_at as CreatedAt
from client_payments cp
         inner join clients c on c.id = cp.client_id
         left join projects p on p.id = cp.project_id
where c.id = :clientId
order by cp.payment_time desc, cp.created_at desc


