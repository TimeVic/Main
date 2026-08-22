with earned_by_project as (
    select
        te.project_id                                                                                        as ProjectId,
        sum(extract(epoch from te.end_time - te.start_time))                                                 as DurationAsEpoch,
        -- Financial reports must only use billable entries with a rate fixed on the time entry.
        sum(fn_calculate_amount(te.start_time, te.end_time, te.hourly_rate, te.is_billable)) as EarnedAmount
    from time_entries te
    where te.workspace_id = :workspaceId
      and te.end_time is not null
      and te.is_billable = true
      and te.hourly_rate is not null
      and te.project_id is not null
      and te.status = 3
    group by te.project_id
),
member_earnings_by_project as (
    select
        te.project_id                                                                                        as ProjectId,
        sum(fn_calculate_amount(te.start_time, te.end_time, coalesce(wmpa.hourly_rate, 0), true)) as TeamCostAmount
    from time_entries te
             inner join workspace_members wm on wm.user_id = te.user_id and wm.workspace_id = te.workspace_id
             left join workspace_member_project_accesses wmpa on wmpa.workspace_member_id = wm.id and wmpa.project_id = te.project_id
    where te.workspace_id = :workspaceId
      and te.end_time is not null
      and te.is_billable = true
      and te.hourly_rate is not null
      and te.project_id is not null
      and te.status = 3
    group by te.project_id
),
relevant_projects as (
    select ProjectId from earned_by_project
    union
    select ProjectId from member_earnings_by_project
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
         inner join relevant_projects rp on rp.ProjectId = p.id
         left join earned_by_project e on e.ProjectId = p.id
         left join clients c on c.id = p.client_id and c.workspace_id = :workspaceId
         left join member_earnings_by_project tc on tc.ProjectId = p.id
where c.id is not null or p.client_id is null
order by p.name
