with time_entry_amounts as (
    select
        p.id as ProjectId,
        c.id as ClientId,
        extract(epoch from sum(te.end_time - te.start_time)) as TotalDurationAsEpoch,
        sum(
            round(
                (te.hourly_rate / 60 / 60)
                    *
                extract(epoch from te.end_time - te.start_time),
                2
            )
        ) as AmountOriginal
    from time_entries te
             left join projects p on p.id = te.project_id
             left join clients c on c.id = p.client_id
    where te.workspace_id = :workspaceId
      and te.user_id = :userId
      and te.is_billable = true
      and te.end_time is not null
      and te.end_time <= :endDate
    group by p.id, c.id
),
report_rows as (
    select ProjectId, ClientId
    from time_entry_amounts

    union

    select pm.project_id as ProjectId, p.client_id as ClientId
    from member_payments pm
             inner join workspace_members wm on wm.id = pm.member_id
             inner join projects p on p.id = pm.project_id
    where wm.user_id = :userId
      and wm.workspace_id = :workspaceId
      and pm.payment_time <= :endDate
    group by pm.project_id, p.client_id
)
select
    rr.ProjectId as ProjectId,
    p.name as ProjectName,
    rr.ClientId as ClientId,
    c.name as ClientName,
    coalesce(tea.TotalDurationAsEpoch, 0) as TotalDurationAsEpoch,
    coalesce(tea.AmountOriginal, 0) as AmountOriginal,
    coalesce((
        select sum(pm.amount)
        from member_payments pm
                 inner join workspace_members wm on wm.id = pm.member_id
                 inner join projects p on p.id = pm.project_id
        where p.client_id = rr.ClientId
          and wm.user_id = :userId
          and wm.workspace_id = :workspaceId
          and pm.payment_time <= :endDate
        group by p.client_id
    ), 0) as PaidAmountByClientOriginal,
    coalesce((
        select sum(pm.amount)
        from member_payments pm
                 inner join workspace_members wm on wm.id = pm.member_id
        where pm.project_id = rr.ProjectId
          and wm.user_id = :userId
          and wm.workspace_id = :workspaceId
          and pm.payment_time <= :endDate
        group by pm.project_id
    ), 0) as PaidAmountByProjectOriginal
from report_rows rr
         left join time_entry_amounts tea
             on (tea.ProjectId = rr.ProjectId or (tea.ProjectId is null and rr.ProjectId is null))
             and (tea.ClientId = rr.ClientId or (tea.ClientId is null and rr.ClientId is null))
         left join projects p on p.id = rr.ProjectId
         left join clients c on c.id = rr.ClientId
