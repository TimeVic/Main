with earned_by_client as (
    select
        p.client_id                                                                                          as ClientId,
        sum(extract(epoch from te.end_time - te.start_time))                                                 as DurationAsEpoch,
        sum(round(te.hourly_rate / 60.0 / 60.0 * extract(epoch from te.end_time - te.start_time), 2))       as EarnedAmount
    from time_entries te
             inner join projects p on p.id = te.project_id
    where te.workspace_id = :workspaceId
      and te.end_time is not null
      and te.is_billable = true
      and cast(te.start_time as date) >= cast(:startDate as date)
      and cast(te.start_time as date) <= cast(:endDate as date)
      and p.client_id is not null
    group by p.client_id
),
received_by_client as (
    select
        cp.client_id                                                                                         as ClientId,
        sum(cp.amount)                                                                                       as ReceivedAmount,
        max(cp.payment_time)                                                                                 as LastPaymentDate
    from client_payments cp
    where cp.workspace_id = :workspaceId
      and cast(cp.payment_time as date) >= cast(:startDate as date)
      and cast(cp.payment_time as date) <= cast(:endDate as date)
    group by cp.client_id
),
relevant_clients as (
    select ClientId from earned_by_client
    union
    select ClientId from received_by_client
)
select
    c.id                                            as ClientId,
    c.name                                          as ClientName,
    coalesce(e.DurationAsEpoch, 0)                  as DurationAsEpoch,
    coalesce(e.EarnedAmount, 0)                     as EarnedAmountOriginal,
    coalesce(r.ReceivedAmount, 0)                   as ReceivedAmountOriginal,
    r.LastPaymentDate                               as LastPaymentDateRaw
from clients c
         inner join relevant_clients rc on rc.ClientId = c.id
         left join earned_by_client e on e.ClientId = c.id
         left join received_by_client r on r.ClientId = c.id
where c.workspace_id = :workspaceId
order by c.name
