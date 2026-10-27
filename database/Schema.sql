CREATE TABLE IF NOT EXISTS __ef_migrations_history (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261019145057_InitialAuth') THEN
    CREATE TABLE users (
        id bigint GENERATED ALWAYS AS IDENTITY,
        email character varying(254) NOT NULL,
        full_name character varying(100) NOT NULL,
        phone character varying(20),
        role character varying(32) NOT NULL,
        status character varying(32) NOT NULL,
        password_hash character varying(256) NOT NULL,
        must_change_password boolean NOT NULL,
        failed_login_count integer NOT NULL,
        locked_until timestamp with time zone,
        last_login_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_users PRIMARY KEY (id),
        CONSTRAINT ck_users_failed_login_count CHECK (failed_login_count >= 0),
        CONSTRAINT ck_users_role CHECK (role IN ('customer', 'officer', 'technician', 'manager', 'admin')),
        CONSTRAINT ck_users_status CHECK (status IN ('active', 'deactivated'))
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261019145057_InitialAuth') THEN
    CREATE UNIQUE INDEX ix_users_email ON users (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261019145057_InitialAuth') THEN
    CREATE INDEX ix_users_role ON users (role);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261019145057_InitialAuth') THEN
    INSERT INTO __ef_migrations_history (migration_id, product_version)
    VALUES ('20261019145057_InitialAuth', '10.0.13');
    END IF;
END $EF$;
COMMIT;

