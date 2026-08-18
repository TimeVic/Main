select
    cp.payment_time as PaymentTime,
    cp.amount as AmountOriginal,
    p.name as ProjectName,
    cp.description as Description
from client_payments cp
         left join projects p on p.id = cp.project_id
where cp.client_id = :clientId
order by cp.payment_time desc, cp.created_at desc;
