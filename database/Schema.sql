CREATE TABLE IF NOT EXISTS __ef_migrations_history (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE asset_categories (
        id bigint GENERATED ALWAYS AS IDENTITY,
        code character varying(20) NOT NULL,
        category_name character varying(100) NOT NULL,
        description character varying(300),
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_asset_categories PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE buildings (
        id bigint GENERATED ALWAYS AS IDENTITY,
        name character varying(100) NOT NULL,
        address character varying(200),
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_buildings PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE case_statuses (
        id bigint NOT NULL,
        name character varying(50) NOT NULL,
        CONSTRAINT pk_case_statuses PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE manufacturers (
        id bigint GENERATED ALWAYS AS IDENTITY,
        name character varying(100) NOT NULL,
        phone_number character varying(20),
        email character varying(254),
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_manufacturers PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE priority_levels (
        id bigint GENERATED ALWAYS AS IDENTITY,
        code character varying(32) NOT NULL,
        name character varying(50) NOT NULL,
        description character varying(300),
        sla_hours integer NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_priority_levels PRIMARY KEY (id),
        CONSTRAINT ck_priority_levels_code CHECK (code IN ('emergency', 'high', 'normal', 'low')),
        CONSTRAINT ck_priority_levels_sla_hours CHECK (sla_hours > 0)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE roles (
        id bigint NOT NULL,
        name character varying(50) NOT NULL,
        CONSTRAINT pk_roles PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE specialties (
        id bigint GENERATED ALWAYS AS IDENTITY,
        name character varying(60) NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_specialties PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE asset_types (
        id bigint GENERATED ALWAYS AS IDENTITY,
        category_id bigint NOT NULL,
        code character varying(20) NOT NULL,
        name character varying(100) NOT NULL,
        description character varying(300),
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_asset_types PRIMARY KEY (id),
        CONSTRAINT fk_asset_types_asset_categories_category_id FOREIGN KEY (category_id) REFERENCES asset_categories (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE floors (
        id bigint GENERATED ALWAYS AS IDENTITY,
        building_id bigint NOT NULL,
        floor_number character varying(10) NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_floors PRIMARY KEY (id),
        CONSTRAINT fk_floors_buildings_building_id FOREIGN KEY (building_id) REFERENCES buildings (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE case_categories (
        id bigint GENERATED ALWAYS AS IDENTITY,
        group_name character varying(60) NOT NULL,
        name character varying(100) NOT NULL,
        description character varying(300),
        is_emergency boolean NOT NULL,
        default_priority_id bigint NOT NULL,
        default_cost_bearer character varying(32) NOT NULL,
        photo_required boolean NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_case_categories PRIMARY KEY (id),
        CONSTRAINT ck_case_categories_default_cost_bearer CHECK (default_cost_bearer IN ('building', 'customer')),
        CONSTRAINT fk_case_categories_priority_levels_default_priority_id FOREIGN KEY (default_priority_id) REFERENCES priority_levels (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE accounts (
        id bigint GENERATED ALWAYS AS IDENTITY,
        username character varying(50) NOT NULL,
        password_hash character varying(256) NOT NULL,
        phone_number character varying(20),
        role_id bigint NOT NULL,
        full_name character varying(100) NOT NULL,
        email character varying(254),
        status character varying(32) NOT NULL,
        must_change_password boolean NOT NULL,
        failed_login_count integer NOT NULL,
        locked_until timestamp with time zone,
        last_login_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_accounts" PRIMARY KEY (id),
        CONSTRAINT ck_accounts_failed_login_count CHECK (failed_login_count >= 0),
        CONSTRAINT ck_accounts_status CHECK (status IN ('active', 'deactivated')),
        CONSTRAINT fk_accounts_roles_role_id FOREIGN KEY (role_id) REFERENCES roles (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE rooms (
        id bigint GENERATED ALWAYS AS IDENTITY,
        floor_id bigint NOT NULL,
        room_number character varying(50) NOT NULL,
        is_common_area boolean NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_rooms PRIMARY KEY (id),
        CONSTRAINT fk_rooms_floors_floor_id FOREIGN KEY (floor_id) REFERENCES floors (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE admins (
        id bigint NOT NULL,
        CONSTRAINT "PK_admins" PRIMARY KEY (id),
        CONSTRAINT fk_admins_accounts_id FOREIGN KEY (id) REFERENCES accounts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE managers (
        id bigint NOT NULL,
        CONSTRAINT "PK_managers" PRIMARY KEY (id),
        CONSTRAINT fk_managers_accounts_id FOREIGN KEY (id) REFERENCES accounts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE staff (
        id bigint NOT NULL,
        CONSTRAINT "PK_staff" PRIMARY KEY (id),
        CONSTRAINT fk_staff_accounts_id FOREIGN KEY (id) REFERENCES accounts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE technicians (
        id bigint NOT NULL,
        hourly_rate numeric(12,2),
        CONSTRAINT "PK_technicians" PRIMARY KEY (id),
        CONSTRAINT ck_technicians_hourly_rate CHECK (hourly_rate IS NULL OR hourly_rate >= 0),
        CONSTRAINT fk_technicians_accounts_id FOREIGN KEY (id) REFERENCES accounts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE assets (
        id bigint GENERATED ALWAYS AS IDENTITY,
        name character varying(150) NOT NULL,
        room_id bigint NOT NULL,
        type_id bigint NOT NULL,
        manufacturer_id bigint,
        has_copyright boolean NOT NULL,
        warranty_end_date date,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_assets PRIMARY KEY (id),
        CONSTRAINT fk_assets_asset_types_type_id FOREIGN KEY (type_id) REFERENCES asset_types (id) ON DELETE RESTRICT,
        CONSTRAINT fk_assets_manufacturers_manufacturer_id FOREIGN KEY (manufacturer_id) REFERENCES manufacturers (id) ON DELETE RESTRICT,
        CONSTRAINT fk_assets_rooms_room_id FOREIGN KEY (room_id) REFERENCES rooms (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE customers (
        id bigint NOT NULL,
        room_id bigint NOT NULL,
        CONSTRAINT "PK_customers" PRIMARY KEY (id),
        CONSTRAINT fk_customers_accounts_id FOREIGN KEY (id) REFERENCES accounts (id) ON DELETE CASCADE,
        CONSTRAINT fk_customers_rooms_room_id FOREIGN KEY (room_id) REFERENCES rooms (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE manager_building_assignments (
        manager_id bigint NOT NULL,
        building_id bigint NOT NULL,
        CONSTRAINT pk_manager_building_assignments PRIMARY KEY (manager_id, building_id),
        CONSTRAINT fk_manager_building_assignments_buildings_building_id FOREIGN KEY (building_id) REFERENCES buildings (id) ON DELETE RESTRICT,
        CONSTRAINT fk_manager_building_assignments_managers_manager_id FOREIGN KEY (manager_id) REFERENCES managers (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE staff_building_assignments (
        staff_id bigint NOT NULL,
        building_id bigint NOT NULL,
        is_primary boolean NOT NULL,
        CONSTRAINT pk_staff_building_assignments PRIMARY KEY (staff_id, building_id),
        CONSTRAINT fk_staff_building_assignments_buildings_building_id FOREIGN KEY (building_id) REFERENCES buildings (id) ON DELETE RESTRICT,
        CONSTRAINT fk_staff_building_assignments_staff_staff_id FOREIGN KEY (staff_id) REFERENCES staff (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE technician_specialties (
        technician_id bigint NOT NULL,
        specialty_id bigint NOT NULL,
        CONSTRAINT pk_technician_specialties PRIMARY KEY (technician_id, specialty_id),
        CONSTRAINT fk_technician_specialties_specialties_specialty_id FOREIGN KEY (specialty_id) REFERENCES specialties (id) ON DELETE RESTRICT,
        CONSTRAINT fk_technician_specialties_technicians_technician_id FOREIGN KEY (technician_id) REFERENCES technicians (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE cases (
        id bigint GENERATED ALWAYS AS IDENTITY,
        requesting_customer_id bigint,
        room_id bigint NOT NULL,
        asset_id bigint,
        category_id bigint NOT NULL,
        title character varying(120) NOT NULL,
        description character varying(2000) NOT NULL,
        status_id bigint NOT NULL,
        priority_level_id bigint NOT NULL,
        cost_bearer character varying(32) NOT NULL,
        staff_id bigint,
        assigned_technician_id bigint,
        submitted_at timestamp with time zone NOT NULL,
        started_at timestamp with time zone,
        due_at timestamp with time zone,
        completed_at timestamp with time zone,
        closed_at timestamp with time zone,
        estimated_completion_hours numeric(6,2),
        actual_completion_hours numeric(6,2),
        outsourced_at timestamp with time zone,
        root_cause character varying(1000),
        parts_used character varying(500),
        hold_reason character varying(32),
        hold_until date,
        rating_stars integer,
        rating_comment character varying(500),
        rated_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_cases PRIMARY KEY (id),
        CONSTRAINT ck_cases_actual_completion_hours CHECK (actual_completion_hours IS NULL OR actual_completion_hours > 0),
        CONSTRAINT ck_cases_cost_bearer CHECK (cost_bearer IN ('building', 'customer')),
        CONSTRAINT ck_cases_estimated_completion_hours CHECK (estimated_completion_hours IS NULL OR estimated_completion_hours > 0),
        CONSTRAINT ck_cases_hold_reason CHECK (hold_reason IN ('waiting_parts', 'waiting_customer', 'waiting_staff_decision', 'waiting_third_party')),
        CONSTRAINT ck_cases_rating_stars CHECK (rating_stars IS NULL OR rating_stars BETWEEN 1 AND 5),
        CONSTRAINT fk_cases_assets_asset_id FOREIGN KEY (asset_id) REFERENCES assets (id) ON DELETE RESTRICT,
        CONSTRAINT fk_cases_case_categories_category_id FOREIGN KEY (category_id) REFERENCES case_categories (id) ON DELETE RESTRICT,
        CONSTRAINT fk_cases_case_statuses_status_id FOREIGN KEY (status_id) REFERENCES case_statuses (id) ON DELETE RESTRICT,
        CONSTRAINT fk_cases_customers_requesting_customer_id FOREIGN KEY (requesting_customer_id) REFERENCES customers (id) ON DELETE RESTRICT,
        CONSTRAINT fk_cases_priority_levels_priority_level_id FOREIGN KEY (priority_level_id) REFERENCES priority_levels (id) ON DELETE RESTRICT,
        CONSTRAINT fk_cases_rooms_room_id FOREIGN KEY (room_id) REFERENCES rooms (id) ON DELETE RESTRICT,
        CONSTRAINT fk_cases_staff_staff_id FOREIGN KEY (staff_id) REFERENCES staff (id) ON DELETE RESTRICT,
        CONSTRAINT fk_cases_technicians_assigned_technician_id FOREIGN KEY (assigned_technician_id) REFERENCES technicians (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE case_images (
        id bigint GENERATED ALWAYS AS IDENTITY,
        case_id bigint NOT NULL,
        kind character varying(32) NOT NULL,
        image character varying(300) NOT NULL,
        notes character varying(500),
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_case_images PRIMARY KEY (id),
        CONSTRAINT ck_case_images_kind CHECK (kind IN ('report', 'before', 'after')),
        CONSTRAINT fk_case_images_cases_case_id FOREIGN KEY (case_id) REFERENCES cases (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE case_status_history (
        id bigint GENERATED ALWAYS AS IDENTITY,
        case_id bigint NOT NULL,
        new_status_id bigint NOT NULL,
        changed_at timestamp with time zone NOT NULL,
        changed_by_id bigint NOT NULL,
        reason character varying(600),
        CONSTRAINT pk_case_status_history PRIMARY KEY (id),
        CONSTRAINT fk_case_status_history_accounts_changed_by_id FOREIGN KEY (changed_by_id) REFERENCES accounts (id) ON DELETE RESTRICT,
        CONSTRAINT fk_case_status_history_case_statuses_new_status_id FOREIGN KEY (new_status_id) REFERENCES case_statuses (id) ON DELETE RESTRICT,
        CONSTRAINT fk_case_status_history_cases_case_id FOREIGN KEY (case_id) REFERENCES cases (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE TABLE technician_rejections (
        technician_id bigint NOT NULL,
        case_id bigint NOT NULL,
        rejected_at timestamp with time zone NOT NULL,
        reason character varying(600) NOT NULL,
        CONSTRAINT pk_technician_rejections PRIMARY KEY (case_id, technician_id, rejected_at),
        CONSTRAINT fk_technician_rejections_cases_case_id FOREIGN KEY (case_id) REFERENCES cases (id) ON DELETE CASCADE,
        CONSTRAINT fk_technician_rejections_technicians_technician_id FOREIGN KEY (technician_id) REFERENCES technicians (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    INSERT INTO case_statuses (id, name)
    VALUES (1, 'pending');
    INSERT INTO case_statuses (id, name)
    VALUES (2, 'approved');
    INSERT INTO case_statuses (id, name)
    VALUES (3, 'assigned');
    INSERT INTO case_statuses (id, name)
    VALUES (4, 'accepted');
    INSERT INTO case_statuses (id, name)
    VALUES (5, 'in_progress');
    INSERT INTO case_statuses (id, name)
    VALUES (6, 'on_hold');
    INSERT INTO case_statuses (id, name)
    VALUES (7, 'completed');
    INSERT INTO case_statuses (id, name)
    VALUES (8, 'closed');
    INSERT INTO case_statuses (id, name)
    VALUES (9, 'reopened');
    INSERT INTO case_statuses (id, name)
    VALUES (10, 'rejected');
    INSERT INTO case_statuses (id, name)
    VALUES (11, 'cancelled');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    INSERT INTO roles (id, name)
    VALUES (1, 'customer');
    INSERT INTO roles (id, name)
    VALUES (2, 'staff');
    INSERT INTO roles (id, name)
    VALUES (3, 'technician');
    INSERT INTO roles (id, name)
    VALUES (4, 'manager');
    INSERT INTO roles (id, name)
    VALUES (5, 'admin');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE UNIQUE INDEX ix_accounts_email ON accounts (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_accounts_role_id ON accounts (role_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE UNIQUE INDEX ix_accounts_username ON accounts (username);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE UNIQUE INDEX ix_asset_categories_code ON asset_categories (code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_asset_types_category_id ON asset_types (category_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE UNIQUE INDEX ix_asset_types_code ON asset_types (code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_assets_manufacturer_id ON assets (manufacturer_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE UNIQUE INDEX ix_assets_room_id_name ON assets (room_id, name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_assets_type_id ON assets (type_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE UNIQUE INDEX ix_buildings_name ON buildings (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_case_categories_default_priority_id ON case_categories (default_priority_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE UNIQUE INDEX ix_case_categories_group_name_name ON case_categories (group_name, name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_case_images_case_id ON case_images (case_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_case_status_history_case_id ON case_status_history (case_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_case_status_history_changed_by_id ON case_status_history (changed_by_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_case_status_history_new_status_id ON case_status_history (new_status_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE UNIQUE INDEX ix_case_statuses_name ON case_statuses (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_cases_asset_id ON cases (asset_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_cases_assigned_technician_id ON cases (assigned_technician_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_cases_category_id ON cases (category_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_cases_priority_level_id ON cases (priority_level_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_cases_requesting_customer_id ON cases (requesting_customer_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_cases_room_id ON cases (room_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_cases_staff_id ON cases (staff_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_cases_status_id ON cases (status_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_cases_submitted_at ON cases (submitted_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_customers_room_id ON customers (room_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE UNIQUE INDEX ix_floors_building_id_floor_number ON floors (building_id, floor_number);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_manager_building_assignments_building_id ON manager_building_assignments (building_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE UNIQUE INDEX ix_manufacturers_name ON manufacturers (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE UNIQUE INDEX ix_priority_levels_code ON priority_levels (code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE UNIQUE INDEX ix_roles_name ON roles (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE UNIQUE INDEX ix_rooms_floor_id_room_number ON rooms (floor_id, room_number);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE UNIQUE INDEX ix_specialties_name ON specialties (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_staff_building_assignments_building_id ON staff_building_assignments (building_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_technician_rejections_technician_id ON technician_rejections (technician_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    CREATE INDEX ix_technician_specialties_specialty_id ON technician_specialties (specialty_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102030509_InitialSchemaErdV5') THEN
    INSERT INTO __ef_migrations_history (migration_id, product_version)
    VALUES ('20261102030509_InitialSchemaErdV5', '10.0.13');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102033133_AddAuditNotificationsAi') THEN
    CREATE TABLE ai_conversations (
        id bigint GENERATED ALWAYS AS IDENTITY,
        context_type character varying(30) NOT NULL,
        context_id bigint,
        user_id bigint NOT NULL,
        CONSTRAINT pk_ai_conversations PRIMARY KEY (id),
        CONSTRAINT fk_ai_conversations_accounts_user_id FOREIGN KEY (user_id) REFERENCES accounts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102033133_AddAuditNotificationsAi') THEN
    CREATE TABLE audit_log_target_operations (
        id bigint NOT NULL,
        name character varying(50) NOT NULL,
        CONSTRAINT pk_audit_log_target_operations PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102033133_AddAuditNotificationsAi') THEN
    CREATE TABLE audit_log_target_types (
        id bigint NOT NULL,
        name character varying(50) NOT NULL,
        CONSTRAINT pk_audit_log_target_types PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102033133_AddAuditNotificationsAi') THEN
    CREATE TABLE notifications (
        id bigint GENERATED ALWAYS AS IDENTITY,
        user_id bigint NOT NULL,
        details character varying(500) NOT NULL,
        sent_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_notifications PRIMARY KEY (id),
        CONSTRAINT fk_notifications_accounts_user_id FOREIGN KEY (user_id) REFERENCES accounts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102033133_AddAuditNotificationsAi') THEN
    CREATE TABLE ai_messages (
        id bigint GENERATED ALWAYS AS IDENTITY,
        conversation_id bigint NOT NULL,
        message character varying(500) NOT NULL,
        reply text,
        sent_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_ai_messages PRIMARY KEY (id),
        CONSTRAINT fk_ai_messages_ai_conversations_conversation_id FOREIGN KEY (conversation_id) REFERENCES ai_conversations (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102033133_AddAuditNotificationsAi') THEN
    CREATE TABLE audit_logs (
        id bigint GENERATED ALWAYS AS IDENTITY,
        admin_id bigint NOT NULL,
        target_type_id bigint NOT NULL,
        operation_id bigint NOT NULL,
        old_value text,
        new_value text,
        occurred_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_audit_logs PRIMARY KEY (id),
        CONSTRAINT fk_audit_logs_admins_admin_id FOREIGN KEY (admin_id) REFERENCES admins (id) ON DELETE RESTRICT,
        CONSTRAINT fk_audit_logs_audit_log_target_operations_operation_id FOREIGN KEY (operation_id) REFERENCES audit_log_target_operations (id) ON DELETE RESTRICT,
        CONSTRAINT fk_audit_logs_audit_log_target_types_target_type_id FOREIGN KEY (target_type_id) REFERENCES audit_log_target_types (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102033133_AddAuditNotificationsAi') THEN
    INSERT INTO audit_log_target_operations (id, name)
    VALUES (1, 'sign_in');
    INSERT INTO audit_log_target_operations (id, name)
    VALUES (2, 'create');
    INSERT INTO audit_log_target_operations (id, name)
    VALUES (3, 'update');
    INSERT INTO audit_log_target_operations (id, name)
    VALUES (4, 'change_role');
    INSERT INTO audit_log_target_operations (id, name)
    VALUES (5, 'deactivate');
    INSERT INTO audit_log_target_operations (id, name)
    VALUES (6, 'reactivate');
    INSERT INTO audit_log_target_operations (id, name)
    VALUES (7, 'reset_password');
    INSERT INTO audit_log_target_operations (id, name)
    VALUES (8, 'unlock');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102033133_AddAuditNotificationsAi') THEN
    INSERT INTO audit_log_target_types (id, name)
    VALUES (1, 'account');
    INSERT INTO audit_log_target_types (id, name)
    VALUES (2, 'priority_level');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102033133_AddAuditNotificationsAi') THEN
    CREATE UNIQUE INDEX ix_ai_conversations_user_id ON ai_conversations (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102033133_AddAuditNotificationsAi') THEN
    CREATE UNIQUE INDEX ix_ai_messages_conversation_id ON ai_messages (conversation_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102033133_AddAuditNotificationsAi') THEN
    CREATE UNIQUE INDEX ix_audit_log_target_operations_name ON audit_log_target_operations (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102033133_AddAuditNotificationsAi') THEN
    CREATE UNIQUE INDEX ix_audit_log_target_types_name ON audit_log_target_types (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102033133_AddAuditNotificationsAi') THEN
    CREATE INDEX ix_audit_logs_admin_id ON audit_logs (admin_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102033133_AddAuditNotificationsAi') THEN
    CREATE INDEX ix_audit_logs_occurred_at ON audit_logs (occurred_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102033133_AddAuditNotificationsAi') THEN
    CREATE INDEX ix_audit_logs_operation_id ON audit_logs (operation_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102033133_AddAuditNotificationsAi') THEN
    CREATE INDEX ix_audit_logs_target_type_id ON audit_logs (target_type_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102033133_AddAuditNotificationsAi') THEN
    CREATE INDEX ix_notifications_user_id_sent_at ON notifications (user_id, sent_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102033133_AddAuditNotificationsAi') THEN
    INSERT INTO __ef_migrations_history (migration_id, product_version)
    VALUES ('20261102033133_AddAuditNotificationsAi', '10.0.13');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102035151_AddAssetRetirementAndAuditTargets') THEN
    ALTER TABLE assets ADD retired_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102035151_AddAssetRetirementAndAuditTargets') THEN
    INSERT INTO audit_log_target_operations (id, name)
    VALUES (9, 'delete');
    INSERT INTO audit_log_target_operations (id, name)
    VALUES (10, 'retire');
    INSERT INTO audit_log_target_operations (id, name)
    VALUES (11, 'import');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102035151_AddAssetRetirementAndAuditTargets') THEN
    INSERT INTO audit_log_target_types (id, name)
    VALUES (3, 'building');
    INSERT INTO audit_log_target_types (id, name)
    VALUES (4, 'floor');
    INSERT INTO audit_log_target_types (id, name)
    VALUES (5, 'room');
    INSERT INTO audit_log_target_types (id, name)
    VALUES (6, 'asset');
    INSERT INTO audit_log_target_types (id, name)
    VALUES (7, 'case_category');
    INSERT INTO audit_log_target_types (id, name)
    VALUES (8, 'asset_category');
    INSERT INTO audit_log_target_types (id, name)
    VALUES (9, 'asset_type');
    INSERT INTO audit_log_target_types (id, name)
    VALUES (10, 'manufacturer');
    INSERT INTO audit_log_target_types (id, name)
    VALUES (11, 'specialty');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102035151_AddAssetRetirementAndAuditTargets') THEN
    INSERT INTO __ef_migrations_history (migration_id, product_version)
    VALUES ('20261102035151_AddAssetRetirementAndAuditTargets', '10.0.13');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102041202_AddCostsNotFixedAndSlaReminders') THEN
    ALTER TABLE cases ADD not_fixed_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102041202_AddCostsNotFixedAndSlaReminders') THEN
    ALTER TABLE cases ADD not_fixed_reason character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102041202_AddCostsNotFixedAndSlaReminders') THEN
    ALTER TABLE cases ADD sla_overdue_notified_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102041202_AddCostsNotFixedAndSlaReminders') THEN
    ALTER TABLE cases ADD sla_warned_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102041202_AddCostsNotFixedAndSlaReminders') THEN
    CREATE TABLE costs (
        id bigint GENERATED ALWAYS AS IDENTITY,
        case_id bigint NOT NULL,
        name character varying(150) NOT NULL,
        description character varying(300),
        amount numeric(14,2) NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_costs PRIMARY KEY (id),
        CONSTRAINT ck_costs_amount CHECK (amount >= 0),
        CONSTRAINT fk_costs_cases_case_id FOREIGN KEY (case_id) REFERENCES cases (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102041202_AddCostsNotFixedAndSlaReminders') THEN
    CREATE INDEX ix_costs_case_id ON costs (case_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102041202_AddCostsNotFixedAndSlaReminders') THEN
    INSERT INTO __ef_migrations_history (migration_id, product_version)
    VALUES ('20261102041202_AddCostsNotFixedAndSlaReminders', '10.0.13');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    ALTER TABLE cases ADD trigger_work_order_id bigint;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE TABLE case_checklists (
        id bigint GENERATED ALWAYS AS IDENTITY,
        name character varying(120) NOT NULL,
        description character varying(500),
        category_id bigint NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_case_checklists PRIMARY KEY (id),
        CONSTRAINT fk_case_checklists_case_categories_category_id FOREIGN KEY (category_id) REFERENCES case_categories (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE TABLE frequency_types (
        id bigint NOT NULL,
        name character varying(50) NOT NULL,
        CONSTRAINT pk_frequency_types PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE TABLE task_statuses (
        id bigint NOT NULL,
        name character varying(50) NOT NULL,
        CONSTRAINT pk_task_statuses PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE TABLE trigger_statuses (
        id bigint NOT NULL,
        name character varying(50) NOT NULL,
        CONSTRAINT pk_trigger_statuses PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE TABLE checklist_tasks (
        id bigint GENERATED ALWAYS AS IDENTITY,
        checklist_id bigint NOT NULL,
        name character varying(200) NOT NULL,
        description character varying(500),
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_checklist_tasks PRIMARY KEY (id),
        CONSTRAINT fk_checklist_tasks_case_checklists_checklist_id FOREIGN KEY (checklist_id) REFERENCES case_checklists (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE TABLE case_tasks (
        id bigint GENERATED ALWAYS AS IDENTITY,
        case_id bigint NOT NULL,
        name character varying(200) NOT NULL,
        description character varying(500),
        status_id bigint NOT NULL,
        repair_case_id bigint,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_case_tasks PRIMARY KEY (id),
        CONSTRAINT fk_case_tasks_cases_case_id FOREIGN KEY (case_id) REFERENCES cases (id) ON DELETE CASCADE,
        CONSTRAINT fk_case_tasks_cases_repair_case_id FOREIGN KEY (repair_case_id) REFERENCES cases (id) ON DELETE SET NULL,
        CONSTRAINT fk_case_tasks_task_statuses_status_id FOREIGN KEY (status_id) REFERENCES task_statuses (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE TABLE maintenance_triggers (
        id bigint GENERATED ALWAYS AS IDENTITY,
        frequency integer NOT NULL,
        frequency_type_id bigint NOT NULL,
        staff_id bigint NOT NULL,
        approving_manager_id bigint,
        status_id bigint NOT NULL,
        last_paused timestamp with time zone,
        last_triggered timestamp with time zone,
        lead_days integer NOT NULL,
        reject_reason character varying(500),
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_maintenance_triggers PRIMARY KEY (id),
        CONSTRAINT ck_maintenance_triggers_frequency CHECK (frequency > 0),
        CONSTRAINT ck_maintenance_triggers_lead_days CHECK (lead_days >= 0),
        CONSTRAINT fk_maintenance_triggers_frequency_types_frequency_type_id FOREIGN KEY (frequency_type_id) REFERENCES frequency_types (id) ON DELETE RESTRICT,
        CONSTRAINT fk_maintenance_triggers_managers_approving_manager_id FOREIGN KEY (approving_manager_id) REFERENCES managers (id) ON DELETE RESTRICT,
        CONSTRAINT fk_maintenance_triggers_staff_staff_id FOREIGN KEY (staff_id) REFERENCES staff (id) ON DELETE RESTRICT,
        CONSTRAINT fk_maintenance_triggers_trigger_statuses_status_id FOREIGN KEY (status_id) REFERENCES trigger_statuses (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE TABLE task_images (
        id bigint GENERATED ALWAYS AS IDENTITY,
        task_id bigint NOT NULL,
        image character varying(300) NOT NULL,
        notes character varying(500),
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_task_images PRIMARY KEY (id),
        CONSTRAINT fk_task_images_case_tasks_task_id FOREIGN KEY (task_id) REFERENCES case_tasks (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE TABLE task_notes (
        id bigint GENERATED ALWAYS AS IDENTITY,
        task_id bigint NOT NULL,
        note character varying(1000) NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_task_notes PRIMARY KEY (id),
        CONSTRAINT fk_task_notes_case_tasks_task_id FOREIGN KEY (task_id) REFERENCES case_tasks (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE TABLE maintenance_work_order_details (
        id bigint GENERATED ALWAYS AS IDENTITY,
        trigger_id bigint NOT NULL,
        checklist_id bigint NOT NULL,
        estimated_hours numeric(6,2),
        priority_level_id bigint NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_maintenance_work_order_details PRIMARY KEY (id),
        CONSTRAINT ck_maintenance_work_order_details_estimated_hours CHECK (estimated_hours IS NULL OR estimated_hours > 0),
        CONSTRAINT fk_maintenance_work_order_details_case_checklists_checklist_id FOREIGN KEY (checklist_id) REFERENCES case_checklists (id) ON DELETE RESTRICT,
        CONSTRAINT fk_maintenance_work_order_details_maintenance_triggers_trigger FOREIGN KEY (trigger_id) REFERENCES maintenance_triggers (id) ON DELETE CASCADE,
        CONSTRAINT fk_maintenance_work_order_details_priority_levels_priority_lev FOREIGN KEY (priority_level_id) REFERENCES priority_levels (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE TABLE maintenance_work_order_assets (
        work_order_detail_id bigint NOT NULL,
        asset_id bigint NOT NULL,
        technician_id bigint,
        start_time date NOT NULL,
        end_time date,
        last_due_date date,
        CONSTRAINT pk_maintenance_work_order_assets PRIMARY KEY (work_order_detail_id, asset_id),
        CONSTRAINT fk_maintenance_work_order_assets_assets_asset_id FOREIGN KEY (asset_id) REFERENCES assets (id) ON DELETE RESTRICT,
        CONSTRAINT fk_maintenance_work_order_assets_maintenance_work_order_detail FOREIGN KEY (work_order_detail_id) REFERENCES maintenance_work_order_details (id) ON DELETE CASCADE,
        CONSTRAINT fk_maintenance_work_order_assets_technicians_technician_id FOREIGN KEY (technician_id) REFERENCES technicians (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    INSERT INTO frequency_types (id, name)
    VALUES (1, 'day');
    INSERT INTO frequency_types (id, name)
    VALUES (2, 'week');
    INSERT INTO frequency_types (id, name)
    VALUES (3, 'month');
    INSERT INTO frequency_types (id, name)
    VALUES (4, 'year');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    INSERT INTO task_statuses (id, name)
    VALUES (1, 'pending');
    INSERT INTO task_statuses (id, name)
    VALUES (2, 'pass');
    INSERT INTO task_statuses (id, name)
    VALUES (3, 'fail');
    INSERT INTO task_statuses (id, name)
    VALUES (4, 'not_applicable');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    INSERT INTO trigger_statuses (id, name)
    VALUES (1, 'pending_approval');
    INSERT INTO trigger_statuses (id, name)
    VALUES (2, 'active');
    INSERT INTO trigger_statuses (id, name)
    VALUES (3, 'rejected');
    INSERT INTO trigger_statuses (id, name)
    VALUES (4, 'paused');
    INSERT INTO trigger_statuses (id, name)
    VALUES (5, 'ended');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE INDEX ix_cases_trigger_work_order_id ON cases (trigger_work_order_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE INDEX ix_case_checklists_category_id ON case_checklists (category_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE UNIQUE INDEX ix_case_checklists_name ON case_checklists (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE INDEX ix_case_tasks_case_id ON case_tasks (case_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE INDEX ix_case_tasks_repair_case_id ON case_tasks (repair_case_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE INDEX ix_case_tasks_status_id ON case_tasks (status_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE INDEX ix_checklist_tasks_checklist_id ON checklist_tasks (checklist_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE UNIQUE INDEX ix_frequency_types_name ON frequency_types (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE INDEX ix_maintenance_triggers_approving_manager_id ON maintenance_triggers (approving_manager_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE INDEX ix_maintenance_triggers_frequency_type_id ON maintenance_triggers (frequency_type_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE INDEX ix_maintenance_triggers_staff_id ON maintenance_triggers (staff_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE INDEX ix_maintenance_triggers_status_id ON maintenance_triggers (status_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE INDEX ix_maintenance_work_order_assets_asset_id ON maintenance_work_order_assets (asset_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE INDEX ix_maintenance_work_order_assets_technician_id ON maintenance_work_order_assets (technician_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE INDEX ix_maintenance_work_order_details_checklist_id ON maintenance_work_order_details (checklist_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE INDEX ix_maintenance_work_order_details_priority_level_id ON maintenance_work_order_details (priority_level_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE INDEX ix_maintenance_work_order_details_trigger_id ON maintenance_work_order_details (trigger_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE INDEX ix_task_images_task_id ON task_images (task_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE INDEX ix_task_notes_task_id ON task_notes (task_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE UNIQUE INDEX ix_task_statuses_name ON task_statuses (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    CREATE UNIQUE INDEX ix_trigger_statuses_name ON trigger_statuses (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    ALTER TABLE cases ADD CONSTRAINT fk_cases_maintenance_work_order_details_trigger_work_order_id FOREIGN KEY (trigger_work_order_id) REFERENCES maintenance_work_order_details (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102074916_AddPreventiveMaintenance') THEN
    INSERT INTO __ef_migrations_history (migration_id, product_version)
    VALUES ('20261102074916_AddPreventiveMaintenance', '10.0.13');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102083411_AddEmergencyFlow') THEN
    ALTER TABLE cases ADD accept_reminded_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102083411_AddEmergencyFlow') THEN
    ALTER TABLE cases ADD acknowledged_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102083411_AddEmergencyFlow') THEN
    ALTER TABLE cases ADD backup_alerted_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102083411_AddEmergencyFlow') THEN
    ALTER TABLE cases ADD emergency_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102083411_AddEmergencyFlow') THEN
    ALTER TABLE cases ADD incident_damage character varying(1000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102083411_AddEmergencyFlow') THEN
    ALTER TABLE cases ADD incident_follow_up character varying(32);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102083411_AddEmergencyFlow') THEN
    ALTER TABLE cases ADD incident_lessons character varying(1000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102083411_AddEmergencyFlow') THEN
    ALTER TABLE cases ADD incident_reported_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102083411_AddEmergencyFlow') THEN
    ALTER TABLE cases ADD made_safe_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102083411_AddEmergencyFlow') THEN
    ALTER TABLE cases ADD manager_alerted_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102083411_AddEmergencyFlow') THEN
    ALTER TABLE cases ADD merged_into_case_id bigint;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102083411_AddEmergencyFlow') THEN
    CREATE INDEX ix_cases_merged_into_case_id ON cases (merged_into_case_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102083411_AddEmergencyFlow') THEN
    ALTER TABLE cases ADD CONSTRAINT ck_cases_incident_follow_up CHECK (incident_follow_up IN ('none', 'preventive_plan', 'replacement_review'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102083411_AddEmergencyFlow') THEN
    ALTER TABLE cases ADD CONSTRAINT fk_cases_cases_merged_into_case_id FOREIGN KEY (merged_into_case_id) REFERENCES cases (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102083411_AddEmergencyFlow') THEN
    UPDATE cases SET emergency_at = submitted_at, acknowledged_at = submitted_at
    WHERE emergency_at IS NULL
      AND priority_level_id = (SELECT id FROM priority_levels WHERE code = 'emergency')
      AND status_id <> (SELECT id FROM case_statuses WHERE name = 'pending');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261102083411_AddEmergencyFlow') THEN
    INSERT INTO __ef_migrations_history (migration_id, product_version)
    VALUES ('20261102083411_AddEmergencyFlow', '10.0.13');
    END IF;
END $EF$;
COMMIT;

