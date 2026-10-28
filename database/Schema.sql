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

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    ALTER TABLE users DROP CONSTRAINT ck_users_role;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    UPDATE users SET role = 'staff' WHERE role = 'officer';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
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
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE TABLE priority_levels (
        id bigint GENERATED ALWAYS AS IDENTITY,
        code character varying(32) NOT NULL,
        name character varying(50) NOT NULL,
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
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE TABLE floors (
        id bigint GENERATED ALWAYS AS IDENTITY,
        building_id bigint NOT NULL,
        name character varying(50) NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_floors PRIMARY KEY (id),
        CONSTRAINT fk_floors_buildings_building_id FOREIGN KEY (building_id) REFERENCES buildings (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE TABLE manager_building_assignments (
        manager_id bigint NOT NULL,
        building_id bigint NOT NULL,
        CONSTRAINT pk_manager_building_assignments PRIMARY KEY (manager_id, building_id),
        CONSTRAINT fk_manager_building_assignments_buildings_building_id FOREIGN KEY (building_id) REFERENCES buildings (id) ON DELETE RESTRICT,
        CONSTRAINT fk_manager_building_assignments_users_manager_id FOREIGN KEY (manager_id) REFERENCES users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE TABLE staff_building_assignments (
        staff_id bigint NOT NULL,
        building_id bigint NOT NULL,
        is_primary boolean NOT NULL,
        CONSTRAINT pk_staff_building_assignments PRIMARY KEY (staff_id, building_id),
        CONSTRAINT fk_staff_building_assignments_buildings_building_id FOREIGN KEY (building_id) REFERENCES buildings (id) ON DELETE RESTRICT,
        CONSTRAINT fk_staff_building_assignments_users_staff_id FOREIGN KEY (staff_id) REFERENCES users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
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
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE TABLE rooms (
        id bigint GENERATED ALWAYS AS IDENTITY,
        floor_id bigint NOT NULL,
        name character varying(100) NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_rooms PRIMARY KEY (id),
        CONSTRAINT fk_rooms_floors_floor_id FOREIGN KEY (floor_id) REFERENCES floors (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE TABLE assets (
        id bigint GENERATED ALWAYS AS IDENTITY,
        room_id bigint NOT NULL,
        name character varying(150) NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_assets PRIMARY KEY (id),
        CONSTRAINT fk_assets_rooms_room_id FOREIGN KEY (room_id) REFERENCES rooms (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE TABLE cases (
        id bigint GENERATED ALWAYS AS IDENTITY,
        requesting_customer_id bigint NOT NULL,
        room_id bigint NOT NULL,
        asset_id bigint,
        category_id bigint NOT NULL,
        title character varying(120) NOT NULL,
        description character varying(2000) NOT NULL,
        status character varying(32) NOT NULL,
        priority_level_id bigint NOT NULL,
        cost_bearer character varying(32) NOT NULL,
        staff_id bigint,
        assigned_technician_id bigint,
        submitted_at timestamp with time zone NOT NULL,
        started_at timestamp with time zone,
        due_at timestamp with time zone,
        completed_at timestamp with time zone,
        closed_at timestamp with time zone,
        actual_hours numeric(5,2),
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
        CONSTRAINT ck_cases_actual_hours CHECK (actual_hours IS NULL OR actual_hours > 0),
        CONSTRAINT ck_cases_cost_bearer CHECK (cost_bearer IN ('building', 'customer')),
        CONSTRAINT ck_cases_hold_reason CHECK (hold_reason IN ('waiting_parts', 'waiting_customer', 'waiting_staff_decision', 'waiting_third_party')),
        CONSTRAINT ck_cases_rating_stars CHECK (rating_stars IS NULL OR rating_stars BETWEEN 1 AND 5),
        CONSTRAINT ck_cases_status CHECK (status IN ('pending', 'approved', 'assigned', 'accepted', 'in_progress', 'on_hold', 'completed', 'closed', 'reopened', 'rejected', 'cancelled')),
        CONSTRAINT fk_cases_assets_asset_id FOREIGN KEY (asset_id) REFERENCES assets (id) ON DELETE RESTRICT,
        CONSTRAINT fk_cases_case_categories_category_id FOREIGN KEY (category_id) REFERENCES case_categories (id) ON DELETE RESTRICT,
        CONSTRAINT fk_cases_priority_levels_priority_level_id FOREIGN KEY (priority_level_id) REFERENCES priority_levels (id) ON DELETE RESTRICT,
        CONSTRAINT fk_cases_rooms_room_id FOREIGN KEY (room_id) REFERENCES rooms (id) ON DELETE RESTRICT,
        CONSTRAINT fk_cases_users_assigned_technician_id FOREIGN KEY (assigned_technician_id) REFERENCES users (id) ON DELETE RESTRICT,
        CONSTRAINT fk_cases_users_requesting_customer_id FOREIGN KEY (requesting_customer_id) REFERENCES users (id) ON DELETE RESTRICT,
        CONSTRAINT fk_cases_users_staff_id FOREIGN KEY (staff_id) REFERENCES users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE TABLE case_assignments (
        id bigint GENERATED ALWAYS AS IDENTITY,
        case_id bigint NOT NULL,
        technician_id bigint NOT NULL,
        assigned_by_id bigint NOT NULL,
        assigned_at timestamp with time zone NOT NULL,
        response character varying(32) NOT NULL,
        responded_at timestamp with time zone,
        decline_reason character varying(600),
        CONSTRAINT pk_case_assignments PRIMARY KEY (id),
        CONSTRAINT ck_case_assignments_response CHECK (response IN ('waiting', 'accepted', 'declined')),
        CONSTRAINT fk_case_assignments_cases_case_id FOREIGN KEY (case_id) REFERENCES cases (id) ON DELETE CASCADE,
        CONSTRAINT fk_case_assignments_users_assigned_by_id FOREIGN KEY (assigned_by_id) REFERENCES users (id) ON DELETE RESTRICT,
        CONSTRAINT fk_case_assignments_users_technician_id FOREIGN KEY (technician_id) REFERENCES users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE TABLE case_images (
        id bigint GENERATED ALWAYS AS IDENTITY,
        case_id bigint NOT NULL,
        kind character varying(32) NOT NULL,
        content text NOT NULL,
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
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE TABLE case_status_history (
        id bigint GENERATED ALWAYS AS IDENTITY,
        case_id bigint NOT NULL,
        from_status character varying(32),
        to_status character varying(32) NOT NULL,
        changed_by_id bigint NOT NULL,
        reason character varying(600),
        changed_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_case_status_history PRIMARY KEY (id),
        CONSTRAINT ck_case_status_history_from_status CHECK (from_status IN ('pending', 'approved', 'assigned', 'accepted', 'in_progress', 'on_hold', 'completed', 'closed', 'reopened', 'rejected', 'cancelled')),
        CONSTRAINT ck_case_status_history_to_status CHECK (to_status IN ('pending', 'approved', 'assigned', 'accepted', 'in_progress', 'on_hold', 'completed', 'closed', 'reopened', 'rejected', 'cancelled')),
        CONSTRAINT fk_case_status_history_cases_case_id FOREIGN KEY (case_id) REFERENCES cases (id) ON DELETE CASCADE,
        CONSTRAINT fk_case_status_history_users_changed_by_id FOREIGN KEY (changed_by_id) REFERENCES users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    ALTER TABLE users ADD CONSTRAINT ck_users_role CHECK (role IN ('customer', 'staff', 'technician', 'manager', 'admin'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE UNIQUE INDEX ix_assets_room_id_name ON assets (room_id, name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE UNIQUE INDEX ix_buildings_name ON buildings (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE INDEX ix_case_assignments_assigned_by_id ON case_assignments (assigned_by_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE INDEX ix_case_assignments_case_id ON case_assignments (case_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE INDEX ix_case_assignments_technician_id ON case_assignments (technician_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE INDEX ix_case_categories_default_priority_id ON case_categories (default_priority_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE UNIQUE INDEX ix_case_categories_group_name_name ON case_categories (group_name, name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE INDEX ix_case_images_case_id ON case_images (case_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE INDEX ix_case_status_history_case_id ON case_status_history (case_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE INDEX ix_case_status_history_changed_by_id ON case_status_history (changed_by_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE INDEX ix_cases_asset_id ON cases (asset_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE INDEX ix_cases_assigned_technician_id ON cases (assigned_technician_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE INDEX ix_cases_category_id ON cases (category_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE INDEX ix_cases_priority_level_id ON cases (priority_level_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE INDEX ix_cases_requesting_customer_id ON cases (requesting_customer_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE INDEX ix_cases_room_id ON cases (room_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE INDEX ix_cases_staff_id ON cases (staff_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE INDEX ix_cases_status ON cases (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE INDEX ix_cases_submitted_at ON cases (submitted_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE UNIQUE INDEX ix_floors_building_id_name ON floors (building_id, name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE INDEX ix_manager_building_assignments_building_id ON manager_building_assignments (building_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE UNIQUE INDEX ix_priority_levels_code ON priority_levels (code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE UNIQUE INDEX ix_rooms_floor_id_name ON rooms (floor_id, name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    CREATE INDEX ix_staff_building_assignments_building_id ON staff_building_assignments (building_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM __ef_migrations_history WHERE "migration_id" = '20261028094941_AddCasesAndRenameOfficerToStaff') THEN
    INSERT INTO __ef_migrations_history (migration_id, product_version)
    VALUES ('20261028094941_AddCasesAndRenameOfficerToStaff', '10.0.13');
    END IF;
END $EF$;
COMMIT;

