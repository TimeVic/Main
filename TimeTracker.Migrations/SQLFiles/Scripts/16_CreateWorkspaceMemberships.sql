insert into workspace_memberships 
(
    workspace_id,
    user_id,
    membership_access_type_id,
    create_time,
    update_time
)
    select
        w.id as workspace_id,
        w.user_id as user_id,
        3 as membership_access_type_id,
        now() as create_time,
        now() as update_time
    from workspaces as w
