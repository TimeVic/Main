with cost_by_member as (
    select
        wm.id                                                                                                as MemberId,
        u.id                                                                                                 as UserId,
        u.user_name                                                                                          as UserName,
        u.email                                                                                              as Email,
        sum(extract(epoch from te.end_time - te.start_time))                                                 as DurationAsEpoch,
        sum(round(coalesce(wmpa.hourly_rate, 0) / 60.0 / 60.0 * extract(epoch from te.end_time - te.start_time), 2)) as CostAmount
    from time_entries te
             inner join workspace_members wm on wm.user_id = te.user_id and wm.workspace_id = te.workspace_id
             inner join users u on u.id = wm.user_id
             left join workspace_member_project_accesses wmpa on wmpa.workspace_member_id = wm.id and wmpa.project_id = te.project_id
    where te.workspace_id = :workspaceId
      and te.end_time is not null
      and te.is_billable = true
      and te.hourly_rate is not null
    group by wm.id, u.id, u.user_name, u.email
),
paidout_by_member as (
    select
        mp.member_id                                                                                         as MemberId,
        sum(mp.amount)                                                                                       as PaidOutAmount,
        max(mp.payment_time)                                                                                 as LastPayoutDate
    from member_payments mp
             inner join workspace_members wm on wm.id = mp.member_id
    where wm.workspace_id = :workspaceId
    group by mp.member_id
),
relevant_members as (
    select MemberId from cost_by_member
    union
    select MemberId from paidout_by_member
)
select
    wm.id                                           as MemberId,
    u.id                                            as UserId,
    u.user_name                                     as UserName,
    u.email                                         as Email,
    coalesce(c.DurationAsEpoch, 0)                  as DurationAsEpoch,
    coalesce(c.CostAmount, 0)                       as CostAmountOriginal,
    coalesce(p.PaidOutAmount, 0)                    as PaidOutAmountOriginal,
    p.LastPayoutDate                                as LastPayoutDateRaw
from workspace_members wm
         inner join users u on u.id = wm.user_id
         inner join relevant_members rm on rm.MemberId = wm.id
         left join cost_by_member c on c.MemberId = wm.id
         left join paidout_by_member p on p.MemberId = wm.id
where wm.workspace_id = :workspaceId
order by u.user_name, u.email
