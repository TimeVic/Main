with earned_by_project as (
    select
        te.project_id                                                                                        as ProjectId,
        sum(extract(epoch from te.end_time - te.start_time))                                                 as DurationAsEpoch,
        sum(round(te.hourly_rate / 60.0 / 60.0 * extract(epoch from te.end_time - te.start_time), 2))       as EarnedAmount
    from time_entries te
    where te.workspace_id = :workspaceId
      and te.end_time is not null
      and te.is_billable = true
      and cast(te.start_time as date) >= cast(:startDate as date)
      and cast(te.start_time as date) <= cast(:endDate as date)
      and te.project_id is not null
    group by te.project_id
),
team_cost_by_project as (
    select
        mp.project_id                                                                                        as ProjectId,
        sum(mp.amount)                                                                                       as TeamCostAmount
    from member_payments mp
             inner join workspace_members wm on wm.id = mp.member_id
    where wm.workspace_id = :workspaceId
      and mp.project_id is not null
      and cast(mp.payment_time as date) >= cast(:startDate as date)
      and cast(mp.payment_time as date) <= cast(:endDate as date)
    group by mp.project_id
)
select
    p.id                                            as ProjectId,
    p.name                                          as ProjectName,
    c.id                                            as ClientId,
    c.name                                          as ClientName,
    coalesce(e.DurationAsEpoch, 0)                  as DurationAsEpoch,
    coalesce(e.EarnedAmount, 0)                     as EarnedAmountOriginal,
    coalesce(tc.TeamCostAmount, 0)                  as TeamCostAmountOriginal
from projects p
         inner join earned_by_project e on e.ProjectId = p.id
         left join clients c on c.id = p.client_id
         left join team_cost_by_project tc on tc.ProjectId = p.id
where p.workspace_id = :workspaceId
order by p.name
