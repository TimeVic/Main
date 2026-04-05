CREATE OR REPLACE FUNCTION fn_split_time_entry_by_day(
    start_utc timestamptz,
    end_utc   timestamptz,
    user_tz   text
)
RETURNS TABLE(
    day date,
    part_start timestamptz,
    part_end   timestamptz,
    duration   interval
) AS $$
BEGIN
RETURN QUERY
    WITH RECURSIVE bounds AS (
        SELECT
            start_utc AT TIME ZONE user_tz AS start_local,
            end_utc   AT TIME ZONE user_tz AS end_local
    ),
    split AS (
        -- First chunk
        SELECT
            start_local AS s,
            LEAST(
                date_trunc('day', start_local) + INTERVAL '1 day',
                end_local
            ) AS e
        FROM bounds

        UNION ALL

        -- Other chunks
        SELECT
            date_trunc('day', s) + INTERVAL '1 day',
            LEAST(
                date_trunc('day', s) + INTERVAL '2 day',
                (SELECT end_local FROM bounds)
            )
        FROM split
        WHERE e < (SELECT end_local FROM bounds)
    )
SELECT
    s::date AS day,                         -- локальная дата
        s AT TIME ZONE user_tz AS part_start,   -- локальный старт
        e AT TIME ZONE user_tz AS part_end,     -- локальный конец
        (e - s) AS duration                     -- длительность куска
FROM split
ORDER BY s;
END;
$$ LANGUAGE plpgsql STABLE;
