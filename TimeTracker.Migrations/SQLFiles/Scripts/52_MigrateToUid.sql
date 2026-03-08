-- МИГРАЦИЯ bigint → uuid с исключениями enum-таблиц
-- Требуется расширение pgcrypto для gen_random_uuid()
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- BEGIN;

-- =========================================
-- Служебные временные таблицы
-- =========================================

CREATE TEMP TABLE _exclude_tables (
    nspname text NOT NULL,
    relname text NOT NULL,
    PRIMARY KEY (nspname, relname)
) ON COMMIT DROP;

-- Список исключений (enum-like): PK/FK остаются int*
INSERT INTO _exclude_tables(nspname, relname) VALUES
                                                  ('public','membership_access_types'),
                                                  ('public','notification_types'),
                                                  ('public','queue_channels'),
                                                  ('public','queue_statuses'),
                                                  ('public','stored_file_statuses'),
                                                  ('public','stored_file_types'),
                                                  ('public','task_priorities'),
                                                  ('public','task_statuses'),
                                                  ('enum','queue_priorities');

CREATE TEMP TABLE _pk_tables (
    parent_nsp   text,
    parent_rel   text,
    parent_oid   oid,
    pk_name      text
) ON COMMIT DROP;

CREATE TEMP TABLE _fk_edges (
    fk_oid           oid,
    fk_name          text,
    child_nsp        text,
    child_rel        text,
    child_oid        oid,
    child_col        text,
    child_attnum     int,
    parent_nsp       text,
    parent_rel       text,
    parent_oid       oid,
    fk_def           text
) ON COMMIT DROP;

CREATE TEMP TABLE _uc_to_recreate (
    child_nsp   text,
    child_rel   text,
    uc_name     text,
    uc_def      text
) ON COMMIT DROP;

CREATE TEMP TABLE _sequences_to_drop (
    seq_nsp text,
    seq_rel text
) ON COMMIT DROP;

-- Для «агрессивной» очистки (опционально)
CREATE TEMP TABLE _uq_candidates (
    parent_nsp text,
    parent_rel text,
    uq_name    text,
    uq_ind_oid oid
) ON COMMIT DROP;

-- =========================================
-- A. Найти родителей с PK=id BIGINT (одиночный) И не в исключениях
--     Добавить id_uuid + ВРЕМЕННЫЙ UNIQUE(id_uuid)
-- =========================================

INSERT INTO _pk_tables(parent_nsp, parent_rel, parent_oid, pk_name)
SELECT
    n.nspname,
    c.relname,
    c.oid,
    con.conname
FROM pg_constraint con
         JOIN pg_class c          ON c.oid = con.conrelid
         JOIN pg_namespace n      ON n.oid = c.relnamespace
         JOIN LATERAL unnest(con.conkey) AS pk_attnum(attnum) ON TRUE
    JOIN pg_attribute a      ON a.attrelid = c.oid AND a.attnum = pk_attnum.attnum
    JOIN pg_type t           ON t.oid = a.atttypid
    LEFT JOIN _exclude_tables x ON x.nspname = n.nspname AND x.relname = c.relname
WHERE con.contype = 'p'
  AND array_length(con.conkey, 1) = 1
  AND a.attname = 'id'
  AND t.typname = 'int8'            -- только BIGINT PK
  AND x.relname IS NULL;            -- исключения НЕ берём

-- Добавим id_uuid, если нет
DO $$
DECLARE
r RECORD;
    exists_uuid boolean;
BEGIN
FOR r IN SELECT * FROM _pk_tables LOOP
SELECT EXISTS (
    SELECT 1
    FROM pg_attribute
    WHERE attrelid = r.parent_oid
      AND attname = 'id_uuid'
      AND attisdropped = FALSE
) INTO exists_uuid;

IF NOT exists_uuid THEN
            EXECUTE format('ALTER TABLE %I.%I ADD COLUMN id_uuid uuid DEFAULT gen_random_uuid() NOT NULL;',
                           r.parent_nsp, r.parent_rel);
END IF;
END LOOP;
END$$;

-- Сразу поставим UNIQUE(id_uuid), чтобы можно было ссылаться из FK
DO $$
DECLARE
r RECORD;
    con_exists boolean;
    uq_name text;
BEGIN
FOR r IN SELECT * FROM _pk_tables LOOP
SELECT EXISTS (
    SELECT 1
    FROM pg_constraint c
             JOIN pg_attribute a ON a.attrelid = c.conrelid
            AND a.attnum = ANY(c.conkey)
    WHERE c.conrelid = r.parent_oid
      AND c.contype IN ('p','u')
      AND a.attname = 'id_uuid'
) INTO con_exists;

IF NOT con_exists THEN
            uq_name := 'UQ_' || r.parent_rel || '_id_uuid';
EXECUTE format('ALTER TABLE %I.%I ADD CONSTRAINT %I UNIQUE (id_uuid);',
               r.parent_nsp, r.parent_rel, uq_name);
END IF;
END LOOP;
END$$;

-- =========================================
-- B. Собрать все FK на этих родителей (на старый id → будем вешать на id_uuid)
-- =========================================

INSERT INTO _fk_edges(fk_oid, fk_name, child_nsp, child_rel, child_oid, child_col, child_attnum,
                      parent_nsp, parent_rel, parent_oid, fk_def)
SELECT
    fk.oid,
    fk.conname,
    n_child.nspname,
    c_child.relname,
    c_child.oid,
    a_child.attname,
    a_child.attnum,
    n_parent.nspname,
    c_parent.relname,
    c_parent.oid,
    pg_get_constraintdef(fk.oid)
FROM pg_constraint fk
         JOIN pg_class c_child      ON c_child.oid = fk.conrelid
         JOIN pg_namespace n_child  ON n_child.oid = c_child.relnamespace
         JOIN pg_class c_parent     ON c_parent.oid = fk.confrelid
         JOIN pg_namespace n_parent ON n_parent.oid = c_parent.relnamespace
         JOIN LATERAL unnest(fk.conkey) AS ck(attnum)     ON TRUE
    JOIN pg_attribute a_child       ON a_child.attrelid = c_child.oid AND a_child.attnum = ck.attnum
    JOIN LATERAL unnest(fk.confkey) AS pk(attnum)    ON TRUE
    JOIN pg_attribute a_parent      ON a_parent.attrelid = c_parent.oid AND a_parent.attnum = pk.attnum
WHERE fk.contype = 'f'
  AND a_parent.attname = 'id'
  AND fk.confrelid IN (SELECT parent_oid FROM _pk_tables)
  AND array_length(fk.conkey, 1) = 1
  AND array_length(fk.confkey, 1) = 1;

-- =========================================
-- B1. Сохранить последовательности (serial для id) родительских таблиц
--     чтобы потом удалить
-- =========================================
INSERT INTO _sequences_to_drop(seq_nsp, seq_rel)
SELECT n.nspname, s.relname
FROM _pk_tables p
         JOIN pg_class t           ON t.oid = p.parent_oid
         JOIN pg_namespace n_t     ON n_t.oid = t.relnamespace
         JOIN pg_attribute a_id    ON a_id.attrelid = t.oid AND a_id.attname = 'id'
         JOIN pg_depend d          ON d.refobjid = t.oid AND d.refobjsubid = a_id.attnum AND d.deptype = 'a'
         JOIN pg_class s           ON s.oid = d.objid AND s.relkind = 'S'
         JOIN pg_namespace n       ON n.oid = s.relnamespace;

-- =========================================
-- B2. Пересоздать дочерние FK на uuid:
--     добавить *_uuid, перелить, переименовать, вернуть FK и UC
-- =========================================
DO $$
DECLARE
f RECORD;
    not_null boolean;
    uc RECORD;
    fk_def_new text;
BEGIN
FOR f IN SELECT * FROM _fk_edges LOOP
                           -- Добавить столбец *_uuid
                           PERFORM 1 FROM pg_attribute
         WHERE attrelid = f.child_oid AND attname = f.child_col || '_uuid' AND attisdropped = FALSE;
IF NOT FOUND THEN
            EXECUTE format('ALTER TABLE %I.%I ADD COLUMN %I uuid;',
                           f.child_nsp, f.child_rel, f.child_col || '_uuid');
END IF;

        -- Перелить ссылки: child.col_uuid = parent.id_uuid там, где child.col = parent.id
EXECUTE format($sql$
                   UPDATE %I.%I c
            SET %I = p.id_uuid
            FROM %I.%I p
            WHERE c.%I = p.id
        $sql$, f.child_nsp, f.child_rel,
               f.child_col || '_uuid',
               f.parent_nsp, f.parent_rel,
               f.child_col);

-- Унаследовать NOT NULL со старой колонки
SELECT a.attnotnull
INTO not_null
FROM pg_attribute a
WHERE a.attrelid = f.child_oid
  AND a.attname  = f.child_col
  AND a.attisdropped = FALSE;

IF not_null THEN
            EXECUTE format('ALTER TABLE %I.%I ALTER COLUMN %I SET NOT NULL;',
                           f.child_nsp, f.child_rel, f.child_col || '_uuid');
END IF;

        -- Сохранить и удалить UNIQUE-ограничения, где участвовала старая колонка
FOR uc IN
SELECT c.conname AS uc_name,
       pg_get_constraintdef(c.oid) AS uc_def
FROM pg_constraint c
WHERE c.conrelid = f.child_oid
  AND c.contype  = 'u'
  AND f.child_attnum = ANY(c.conkey)
    LOOP
INSERT INTO _uc_to_recreate(child_nsp, child_rel, uc_name, uc_def)
VALUES (f.child_nsp, f.child_rel, uc.uc_name, uc.uc_def);
EXECUTE format('ALTER TABLE %I.%I DROP CONSTRAINT %I;',
               f.child_nsp, f.child_rel, uc.uc_name);
END LOOP;

        -- Снести старый FK
EXECUTE format('ALTER TABLE %I.%I DROP CONSTRAINT %I;',
               f.child_nsp, f.child_rel, f.fk_name);

-- Удалить старую bigint-колонку и переименовать *_uuid обратно в старое имя
EXECUTE format('ALTER TABLE %I.%I DROP COLUMN %I;',
               f.child_nsp, f.child_rel, f.child_col);
EXECUTE format('ALTER TABLE %I.%I RENAME COLUMN %I TO %I;',
               f.child_nsp, f.child_rel,
               f.child_col || '_uuid', f.child_col);

-- Пересобрать определение FK, заменив REFERENCES (... id) → (... id_uuid)
fk_def_new := regexp_replace(
                         f.fk_def,
                         '\yREFERENCES\s+([A-Za-z_"][A-Za-z0-9_".]*)\s*\(\s*id\s*\)',
                         'REFERENCES \1 (id_uuid)',
                         'i'
                     );

        -- Вернуть FK под тем же именем
EXECUTE format('ALTER TABLE %I.%I ADD CONSTRAINT %I %s;',
               f.child_nsp, f.child_rel, f.fk_name, fk_def_new);

-- Восстановить ранее удалённые UNIQUE (теперь уже на колонку с прежним именем)
FOR uc IN
SELECT * FROM _uc_to_recreate
WHERE child_nsp = f.child_nsp AND child_rel = f.child_rel
    LOOP
BEGIN
EXECUTE format('ALTER TABLE %I.%I ADD CONSTRAINT %I %s;',
               uc.child_nsp, uc.child_rel, uc.uc_name, uc.uc_def);
EXCEPTION WHEN duplicate_object THEN
                NULL;
END;
DELETE FROM _uc_to_recreate
WHERE child_nsp = uc.child_nsp AND child_rel = uc.child_rel
                               AND uc_name   = uc.uc_name;
END LOOP;
END LOOP;
END$$;

-- =========================================
-- C. У родителей: переключить PK на UUID
--     DROP CONSTRAINT PK, DROP COLUMN id, RENAME id_uuid→id, ADD PK(id)
-- =========================================
DO $$
DECLARE
r RECORD;
BEGIN
FOR r IN SELECT * FROM _pk_tables LOOP
    EXECUTE format('ALTER TABLE %I.%I DROP CONSTRAINT %I;',
                       r.parent_nsp, r.parent_rel, r.pk_name);

EXECUTE format('ALTER TABLE %I.%I DROP COLUMN id;',
               r.parent_nsp, r.parent_rel);

EXECUTE format('ALTER TABLE %I.%I RENAME COLUMN id_uuid TO id;',
               r.parent_nsp, r.parent_rel);

EXECUTE format('ALTER TABLE %I.%I ADD CONSTRAINT %I PRIMARY KEY (id);',
               r.parent_nsp, r.parent_rel, r.pk_name);
END LOOP;
END$$;

-- =========================================
-- C2. Безопасная очистка временных UNIQUE(id_uuid):
--     удаляем ТОЛЬКО если на индекс нет зависимостей (FK).
-- =========================================
DO $$
DECLARE
r RECORD;
    uq_name text;
    uq_oid oid;
    dep_exists boolean;
BEGIN
FOR r IN SELECT * FROM _pk_tables LOOP
                           uq_name := 'UQ_' || r.parent_rel || '_id_uuid';

-- Найти OID индекса, который поддерживает этот UNIQUE
SELECT c.conindid
INTO uq_oid
FROM pg_constraint c
         JOIN pg_class t ON t.oid = c.conrelid
         JOIN pg_namespace n ON n.oid = t.relnamespace
WHERE n.nspname = r.parent_nsp
  AND t.relname = r.parent_rel
  AND c.conname = uq_name
  AND c.contype = 'u';

IF uq_oid IS NULL THEN
            CONTINUE;
END IF;

        -- Есть ли зависящие объекты (например, FK)?
SELECT EXISTS (
    SELECT 1
    FROM pg_depend d
    WHERE d.refobjid = uq_oid
) INTO dep_exists;

IF NOT dep_exists THEN
            EXECUTE format('ALTER TABLE %I.%I DROP CONSTRAINT %I;',
                           r.parent_nsp, r.parent_rel, uq_name);
ELSE
            -- Оставляем UNIQUE, т.к. на нём висят FK; это безопасно.
            -- При желании см. «агрессивный» блок ниже.
            NULL;
END IF;
END LOOP;
END$$;

-- =========================================
-- D. Удаление осиротевших последовательностей (старых serial)
-- =========================================
DO $$
DECLARE
s RECORD;
BEGIN
FOR s IN SELECT * FROM _sequences_to_drop LOOP
                           IF EXISTS (
            SELECT 1 FROM pg_class c
            JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = s.seq_nsp AND c.relname = s.seq_rel AND c.relkind = 'S'
        ) THEN
            EXECUTE format('DROP SEQUENCE %I.%I;', s.seq_nsp, s.seq_rel);
END IF;
END LOOP;
END$$;

-- COMMIT;

-- =========================================
-- (ОПЦИОНАЛЬНО) Агрессивная очистка дублирующих UNIQUE:
-- 1) Найти UNIQUE(id_uuid), на которые ссылаются FK (через индекс).
-- 2) Для каждого: собрать зависимые FK, удалить их, удалить UNIQUE, пересоздать FK на PK(id).
-- *** Выполняйте ТОЛЬКО если хотите избавиться от всех временных UNIQUE. ***
-- =========================================
/*
BEGIN;

-- 1) Кандидаты на удаление
INSERT INTO _uq_candidates(parent_nsp, parent_rel, uq_name, uq_ind_oid)
SELECT n.nspname, t.relname, c.conname, c.conindid
FROM pg_constraint c
JOIN _pk_tables p          ON p.parent_oid = c.conrelid
JOIN pg_class t            ON t.oid = c.conrelid
JOIN pg_namespace n        ON n.oid = t.relnamespace
WHERE c.contype = 'u'
  AND c.conname = 'UQ_' || t.relname || '_id_uuid'
  AND EXISTS (SELECT 1 FROM pg_depend d WHERE d.refobjid = c.conindid);

-- 2) Для каждого такого UNIQUE найти зависимые FK и пересоздать их на PK(id)
DO $$
DECLARE
    r RECORD;
    dep RECORD;
    fk_def text;
    fk_def_new text;
BEGIN
    FOR r IN SELECT * FROM _uq_candidates LOOP
        -- Найти все FK, зависящие от uq_ind_oid
        FOR dep IN
            SELECT fk.oid AS fk_oid, fk.conname AS fk_name,
                   n_child.nspname AS child_nsp, c_child.relname AS child_rel,
                   pg_get_constraintdef(fk.oid) AS fk_def
            FROM pg_depend d
            JOIN pg_constraint fk      ON fk.oid = d.objid AND fk.contype='f'
            JOIN pg_class c_child      ON c_child.oid = fk.conrelid
            JOIN pg_namespace n_child  ON n_child.oid = c_child.relnamespace
            WHERE d.refobjid = r.uq_ind_oid
        LOOP
            fk_def := dep.fk_def;
            -- Заменим ссылку REFERENCES ... (id_uuid) -> (id)
            fk_def_new := regexp_replace(
                              fk_def,
                              '\yREFERENCES\s+([A-Za-z_"][A-Za-z0-9_".]*)\s*\(\s*id_uuid\s*\)',
                              'REFERENCES \1 (id)',
                              'i'
                          );

            -- Удалить и пересоздать FK
            EXECUTE format('ALTER TABLE %I.%I DROP CONSTRAINT %I;',
                           dep.child_nsp, dep.child_rel, dep.fk_name);
            EXECUTE format('ALTER TABLE %I.%I ADD CONSTRAINT %I %s;',
                           dep.child_nsp, dep.child_rel, dep.fk_name, fk_def_new);
        END LOOP;

        -- Теперь можно удалить UNIQUE
        EXECUTE format('ALTER TABLE %I.%I DROP CONSTRAINT %I;',
                       r.parent_nsp, r.parent_rel, r.uq_name);
    END LOOP;
END$$;

COMMIT;
*/

-- =========================================
-- Пост‑проверки (запускать после COMMIT)
-- =========================================

-- 1) Убедиться, что среди родителей больше нет PK=id BIGINT (результатов быть не должно)
-- SELECT n.nspname, c.relname
-- FROM pg_class c
-- JOIN pg_namespace n ON n.oid = c.relnamespace
-- JOIN pg_constraint con ON con.conrelid = c.oid AND con.contype='p'
-- JOIN LATERAL unnest(con.conkey) AS pk(attnum) ON TRUE
-- JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = pk.attnum
-- JOIN pg_type t ON t.oid = a.atttypid
-- WHERE a.attname='id' AND t.typname = 'int8';

-- 2) Проверить, что все FK на мигрированных родителей теперь uuid (результатов быть не должно)
-- SELECT con.conname, n_child.nspname||'.'||c_child.relname AS child_tbl,
--        a_child.attname AS child_col, a_t.typname AS col_type, pg_get_constraintdef(con.oid) AS def
-- FROM pg_constraint con
-- JOIN pg_class c_child ON c_child.oid = con.conrelid
-- JOIN pg_namespace n_child ON n_child.oid = c_child.relnamespace
-- JOIN LATERAL unnest(con.conkey) ck(attnum) ON TRUE
-- JOIN pg_attribute a_child ON a_child.attrelid = c_child.oid AND a_child.attnum = ck.attnum
-- JOIN pg_type a_t ON a_t.oid = a_child.atttypid
-- WHERE con.contype='f' AND a_t.typname <> 'uuid';

-- 3) Проверить, что enum-таблицы остались в исходном int-состоянии
-- SELECT n.nspname, c.relname, t.typname AS pk_type
-- FROM _exclude_tables x
-- JOIN pg_class c ON c.relname = x.relname
-- JOIN pg_namespace n ON n.nspname = x.nspname AND n.oid = c.relnamespace
-- JOIN pg_constraint
