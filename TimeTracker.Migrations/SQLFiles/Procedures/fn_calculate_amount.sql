CREATE OR REPLACE FUNCTION fn_calculate_amount(
    duration_seconds double precision,
    hourly_rate numeric,
    is_billable boolean DEFAULT true
)
RETURNS numeric AS $$
BEGIN
    IF is_billable IS NOT TRUE OR duration_seconds IS NULL OR hourly_rate IS NULL THEN
        RETURN 0;
    END IF;
    RETURN round(cast((duration_seconds / 3600.0) * hourly_rate as numeric), 2);
END;
$$ LANGUAGE plpgsql IMMUTABLE;

CREATE OR REPLACE FUNCTION fn_calculate_amount(
    duration interval,
    hourly_rate numeric,
    is_billable boolean DEFAULT true
)
RETURNS numeric AS $$
BEGIN
    IF is_billable IS NOT TRUE OR duration IS NULL OR hourly_rate IS NULL THEN
        RETURN 0;
    END IF;
    RETURN round(cast((extract(epoch from duration) / 3600.0) * hourly_rate as numeric), 2);
END;
$$ LANGUAGE plpgsql IMMUTABLE;

CREATE OR REPLACE FUNCTION fn_calculate_amount(
    start_time timestamp,
    end_time timestamp,
    hourly_rate numeric,
    is_billable boolean DEFAULT true
)
RETURNS numeric AS $$
BEGIN
    IF is_billable IS NOT TRUE OR start_time IS NULL OR end_time IS NULL OR hourly_rate IS NULL THEN
        RETURN 0;
    END IF;
    RETURN round(cast((extract(epoch from (end_time - start_time)) / 3600.0) * hourly_rate as numeric), 2);
END;
$$ LANGUAGE plpgsql IMMUTABLE;

CREATE OR REPLACE FUNCTION fn_calculate_amount(
    start_time timestamptz,
    end_time timestamptz,
    hourly_rate numeric,
    is_billable boolean DEFAULT true
)
RETURNS numeric AS $$
BEGIN
    IF is_billable IS NOT TRUE OR start_time IS NULL OR end_time IS NULL OR hourly_rate IS NULL THEN
        RETURN 0;
    END IF;
    RETURN round(cast((extract(epoch from (end_time - start_time)) / 3600.0) * hourly_rate as numeric), 2);
END;
$$ LANGUAGE plpgsql IMMUTABLE;
