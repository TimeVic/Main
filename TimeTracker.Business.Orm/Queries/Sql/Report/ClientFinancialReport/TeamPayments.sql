select
    payments.payment_time as PaymentTime,
    payments.amount as AmountOriginal,
    payments.project_name as ProjectName,
    payments.description as Description,
    payments.created_at as CreatedAt
from (
         select
             cp.payment_time,
             cp.amount,
             p.name as project_name,
             cp.description,
             cp.created_at
         from client_payments cp
                  inner join clients c on c.id = cp.client_id
                  left join projects p on p.id = cp.project_id
         where c.id = :clientId

         union all

         select
             mp.payment_time,
             mp.amount,
             p.name as project_name,
             mp.description,
             mp.created_at
         from member_payments mp
                  inner join projects p on p.id = mp.project_id
         where p.client_id = :clientId
     ) payments
order by payments.payment_time desc, payments.created_at desc


