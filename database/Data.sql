-- ============================================================================
-- SmartMaint — seed data (Data.sql)
-- Single source of seed data: run automatically by the API on local start
-- (Database__MigrateAndSeedOnStartup=true) and submitted with Report 4/5.
-- Tables follow ERD v5 (Drive, logical page); differences: docs/database/ERD_v5_Thay_Doi_Khi_Code.md.
--
-- Rules:
--   * Idempotent: every INSERT ends with ON CONFLICT ... DO NOTHING (or is guarded), so the file can run any number of times.
--   * Never insert id values (identity ALWAYS); reference other rows by their natural key (username, name, code).
--   * Lookup tables (roles, case_statuses) are filled by the migration, not here.
--   * One section per table, in FK order. Each flow owner appends the section for their own tables.
--   * DEMO DATA ONLY — no real people, no real passwords.
--
-- Demo accounts (sign in with the username) — all use the password:  Smartmaint@2026
--   admin        Administrator
--   manager      Facility Manager (both buildings)
--   staff        Facility Staff, primary for SmartMaint Tower A
--   tuan         Facility Staff, primary for Riverside Office B, backup for Tower A
--   technician   Maintenance Technician (electrical, HVAC)
--   duc          Maintenance Technician (electrical)
--   long         Maintenance Technician (plumbing, general repair)
--   customer     Customer, Tower A room 501 (An Phat Co.)
--   bao          Customer, Tower A room 502 (Lumen Studio)
--   lan          Customer, Tower A room 1201 (Vista Law)
--   hoa          Customer, Riverside Office B room 301 (Minh Long JSC)
--   newuser      Customer, Riverside Office B room 302, must_change_password = true (to demo AUTH-04)
-- ============================================================================

-- ---------------------------------------------------------------------------
-- buildings, floors, rooms  (owner: Huy — F1)
-- is_common_area: lobby, corridor, restroom, meeting room — every customer of the building may report there.
-- ---------------------------------------------------------------------------
INSERT INTO buildings (name, address)
VALUES ('SmartMaint Tower A', '12 Nguyen Hue, District 1'),
       ('Riverside Office B', '88 Ton Duc Thang, District 1')
ON CONFLICT (name) DO NOTHING;

INSERT INTO floors (building_id, floor_number)
SELECT b.id, f.floor_number
FROM (VALUES ('SmartMaint Tower A', 'G'),
             ('SmartMaint Tower A', '5'),
             ('SmartMaint Tower A', '12'),
             ('Riverside Office B', '3')) AS f(building, floor_number)
JOIN buildings b ON b.name = f.building
ON CONFLICT (building_id, floor_number) DO NOTHING;

INSERT INTO rooms (floor_id, room_number, is_common_area)
SELECT fl.id, r.room_number, r.common
FROM (VALUES ('SmartMaint Tower A', 'G',  'Lobby',           true),
             ('SmartMaint Tower A', 'G',  'Restroom G',      true),
             ('SmartMaint Tower A', '5',  '501',             false),
             ('SmartMaint Tower A', '5',  '502',             false),
             ('SmartMaint Tower A', '5',  'Corridor 5',      true),
             ('SmartMaint Tower A', '12', '1201',            false),
             ('SmartMaint Tower A', '12', 'Meeting room 12', true),
             ('Riverside Office B', '3',  '301',             false),
             ('Riverside Office B', '3',  '302',             false)) AS r(building, floor_number, room_number, common)
JOIN buildings b ON b.name = r.building
JOIN floors fl ON fl.building_id = b.id AND fl.floor_number = r.floor_number
ON CONFLICT (floor_id, room_number) DO NOTHING;

-- ---------------------------------------------------------------------------
-- accounts + actor tables (customers, staff, technicians, managers, admins)  (owner: Trí — core)
-- password_hash = ASP.NET Core Identity v3 (PBKDF2-HMAC-SHA512, 100 000 iterations)
-- Every account also gets a row (same id) in the table of its role.
-- ---------------------------------------------------------------------------
INSERT INTO accounts (username, full_name, email, phone_number, role_id, status, password_hash, must_change_password, failed_login_count)
SELECT a.username, a.full_name, a.email, NULL, r.id, 'active', a.hash, a.must_change, 0
FROM (VALUES
    ('admin',      'System Administrator',  'admin@smartmaint.test',      'admin',      false,
     'AQAAAAIAAYagAAAAECzsqwdj++qsn/kHW6W2sDWOqw+6uEdg6WhjSwdlhYf0yZJONrgOnyikQjbVPluNOg=='),
    ('manager',    'Demo Facility Manager', 'manager@smartmaint.test',    'manager',    false,
     'AQAAAAIAAYagAAAAEKJ0YOEksCjFB/AuvGDzBw9cDZV6r+Aw6TQ5H3IXY6rRK/ib0dJAM2F3S/E6ZVTCcA=='),
    ('staff',      'Demo Facility Staff',   'staff@smartmaint.test',      'staff',      false,
     'AQAAAAIAAYagAAAAEA0mgM6d1JJNIWBoFNFIEyvDF+GUDFklW9VZIoY6HP8tnqvsBq1HuTPy++blx3jRCg=='),
    ('tuan',       'Nguyen Van Tuan',       'tuan@smartmaint.test',       'staff',      false,
     'AQAAAAIAAYagAAAAEA0mgM6d1JJNIWBoFNFIEyvDF+GUDFklW9VZIoY6HP8tnqvsBq1HuTPy++blx3jRCg=='),
    ('technician', 'Demo Technician',       'technician@smartmaint.test', 'technician', false,
     'AQAAAAIAAYagAAAAEHZA3bhJRxkdFqUtQQ0KwuwElOGqJUqQP+W4QRZNS68j3k9jq4EZBwc/E7tmZwyeaA=='),
    ('duc',        'Pham Minh Duc',         'duc@smartmaint.test',        'technician', false,
     'AQAAAAIAAYagAAAAEHZA3bhJRxkdFqUtQQ0KwuwElOGqJUqQP+W4QRZNS68j3k9jq4EZBwc/E7tmZwyeaA=='),
    ('long',       'Vo Thanh Long',         'long@smartmaint.test',       'technician', false,
     'AQAAAAIAAYagAAAAEHZA3bhJRxkdFqUtQQ0KwuwElOGqJUqQP+W4QRZNS68j3k9jq4EZBwc/E7tmZwyeaA=='),
    ('customer',   'Demo Customer',         'customer@smartmaint.test',   'customer',   false,
     'AQAAAAIAAYagAAAAEPsUi5sdT96poC9UdjfZNigi6qmwILSyfugb8LnNojE3ZI9BVv6AC/+XdMxCKtFg7w=='),
    ('bao',        'Tran Quoc Bao',         'bao@smartmaint.test',        'customer',   false,
     'AQAAAAIAAYagAAAAEPsUi5sdT96poC9UdjfZNigi6qmwILSyfugb8LnNojE3ZI9BVv6AC/+XdMxCKtFg7w=='),
    ('lan',        'Do Ngoc Lan',           'lan@smartmaint.test',        'customer',   false,
     'AQAAAAIAAYagAAAAEPsUi5sdT96poC9UdjfZNigi6qmwILSyfugb8LnNojE3ZI9BVv6AC/+XdMxCKtFg7w=='),
    ('hoa',        'Le Thi Hoa',            'hoa@smartmaint.test',        'customer',   false,
     'AQAAAAIAAYagAAAAEPsUi5sdT96poC9UdjfZNigi6qmwILSyfugb8LnNojE3ZI9BVv6AC/+XdMxCKtFg7w=='),
    ('newuser',    'Demo New Customer',     'newuser@smartmaint.test',    'customer',   true,
     'AQAAAAIAAYagAAAAEHiuxRI8CvJ7xMnmYzN7viZFj+kAQr6Lv1NcH4KM8pcKuiKPU4Nhyj44vcPci27zFQ==')
) AS a(username, full_name, email, role, must_change, hash)
JOIN roles r ON r.name = a.role
ON CONFLICT (username) DO NOTHING;

INSERT INTO admins (id)
SELECT a.id FROM accounts a JOIN roles r ON r.id = a.role_id WHERE r.name = 'admin'
ON CONFLICT (id) DO NOTHING;

INSERT INTO managers (id)
SELECT a.id FROM accounts a JOIN roles r ON r.id = a.role_id WHERE r.name = 'manager'
ON CONFLICT (id) DO NOTHING;

INSERT INTO staff (id)
SELECT a.id FROM accounts a JOIN roles r ON r.id = a.role_id WHERE r.name = 'staff'
ON CONFLICT (id) DO NOTHING;

-- hourly_rate in VND (E9)
INSERT INTO technicians (id, hourly_rate)
SELECT a.id, t.rate
FROM (VALUES ('technician', 90000), ('duc', 80000), ('long', 75000)) AS t(username, rate)
JOIN accounts a ON a.username = t.username
ON CONFLICT (id) DO NOTHING;

INSERT INTO customers (id, room_id)
SELECT a.id, rm.id
FROM (VALUES ('customer', 'SmartMaint Tower A', '501'),
             ('bao',      'SmartMaint Tower A', '502'),
             ('lan',      'SmartMaint Tower A', '1201'),
             ('hoa',      'Riverside Office B', '301'),
             ('newuser',  'Riverside Office B', '302')) AS c(username, building, room_number)
JOIN accounts a ON a.username = c.username
JOIN buildings b ON b.name = c.building
JOIN floors fl ON fl.building_id = b.id
JOIN rooms rm ON rm.floor_id = fl.id AND rm.room_number = c.room_number
ON CONFLICT (id) DO NOTHING;

-- ---------------------------------------------------------------------------
-- specialties, technician_specialties  (owner: Trí — core)
-- ---------------------------------------------------------------------------
INSERT INTO specialties (name)
VALUES ('Electrical'), ('Plumbing'), ('HVAC'), ('Elevator'), ('General repair')
ON CONFLICT (name) DO NOTHING;

INSERT INTO technician_specialties (technician_id, specialty_id)
SELECT a.id, s.id
FROM (VALUES ('technician', 'Electrical'),
             ('technician', 'HVAC'),
             ('duc',        'Electrical'),
             ('long',       'Plumbing'),
             ('long',       'General repair')) AS t(username, specialty)
JOIN accounts a ON a.username = t.username
JOIN specialties s ON s.name = t.specialty
ON CONFLICT (technician_id, specialty_id) DO NOTHING;

-- ---------------------------------------------------------------------------
-- asset_categories, asset_types, manufacturers, assets  (owner: Huy — F1)
-- has_copyright = only the manufacturer may service it (E4).
-- ---------------------------------------------------------------------------
INSERT INTO asset_categories (code, category_name, description)
VALUES ('HVAC',  'Air conditioning and ventilation', NULL),
       ('ELEC',  'Electrical',                       NULL),
       ('PLUMB', 'Plumbing',                         NULL),
       ('LIFT',  'Elevators',                        NULL)
ON CONFLICT (code) DO NOTHING;

INSERT INTO asset_types (category_id, code, name, description)
SELECT c.id, t.code, t.name, NULL
FROM (VALUES ('HVAC',  'AC-SPLIT', 'Split air conditioner'),
             ('ELEC',  'LIGHT',    'LED ceiling light'),
             ('ELEC',  'DB',       'Distribution board'),
             ('PLUMB', 'HEATER',   'Water heater'),
             ('LIFT',  'ELEVATOR', 'Passenger elevator')) AS t(category, code, name)
JOIN asset_categories c ON c.code = t.category
ON CONFLICT (code) DO NOTHING;

INSERT INTO manufacturers (name, phone_number, email)
VALUES ('Daikin',              NULL, NULL),
       ('Panasonic',           NULL, NULL),
       ('Ariston',             NULL, NULL),
       ('Mitsubishi Electric', NULL, NULL)
ON CONFLICT (name) DO NOTHING;

INSERT INTO assets (room_id, name, type_id, manufacturer_id, has_copyright, warranty_end_date)
SELECT rm.id, a.name, t.id, m.id, a.copyright, a.warranty::date
FROM (VALUES ('SmartMaint Tower A', '501',        'Split AC Daikin 2HP (#AC-501)',      'AC-SPLIT', 'Daikin',              false, '2027-06-30'),
             ('SmartMaint Tower A', '501',        'LED ceiling panel (#LT-501)',        'LIGHT',    NULL,                  false, NULL),
             ('SmartMaint Tower A', '502',        'Split AC Panasonic 1.5HP (#AC-502)', 'AC-SPLIT', 'Panasonic',           false, '2026-03-31'),
             ('SmartMaint Tower A', 'Restroom G', 'Water heater Ariston 30L (#WH-G1)',  'HEATER',   'Ariston',             false, NULL),
             ('SmartMaint Tower A', '1201',       'Distribution board DB-12 (#EL-12)',  'DB',       NULL,                  false, NULL),
             ('SmartMaint Tower A', 'Lobby',      'Passenger elevator 1 (#EV-01)',      'ELEVATOR', 'Mitsubishi Electric', true,  '2028-12-31')
) AS a(building, room_number, name, type_code, maker, copyright, warranty)
JOIN buildings b ON b.name = a.building
JOIN floors fl ON fl.building_id = b.id
JOIN rooms rm ON rm.floor_id = fl.id AND rm.room_number = a.room_number
JOIN asset_types t ON t.code = a.type_code
LEFT JOIN manufacturers m ON m.name = a.maker
ON CONFLICT (room_id, name) DO NOTHING;

-- ---------------------------------------------------------------------------
-- staff_building_assignments, manager_building_assignments  (owner: Huy — F1)
-- A building has several staff; is_primary marks the main one.
-- ---------------------------------------------------------------------------
INSERT INTO staff_building_assignments (staff_id, building_id, is_primary)
SELECT a.id, b.id, s.is_primary
FROM (VALUES ('staff', 'SmartMaint Tower A', true),
             ('tuan',  'Riverside Office B', true),
             ('tuan',  'SmartMaint Tower A', false)) AS s(username, building, is_primary)
JOIN accounts a ON a.username = s.username
JOIN buildings b ON b.name = s.building
ON CONFLICT (staff_id, building_id) DO NOTHING;

INSERT INTO manager_building_assignments (manager_id, building_id)
SELECT a.id, b.id
FROM accounts a CROSS JOIN buildings b
WHERE a.username = 'manager'
ON CONFLICT (manager_id, building_id) DO NOTHING;

-- ---------------------------------------------------------------------------
-- priority_levels, case_categories  (owner: Trí — F3 core; categories come from F1 later)
-- sla_hours is wall-clock for now (office-hour counting: BR-08, O04).
-- ---------------------------------------------------------------------------
INSERT INTO priority_levels (code, name, description, sla_hours)
VALUES ('emergency', 'Emergency', 'Danger to people or the building; handled by F4', 2),
       ('high',      'High',      'A whole room or service is down',                  8),
       ('normal',    'Normal',    'Something is broken but work can go on',          24),
       ('low',       'Low',       'Cosmetic or can wait',                            72)
ON CONFLICT (code) DO NOTHING;

INSERT INTO case_categories (group_name, name, description, is_emergency, default_priority_id, default_cost_bearer, photo_required)
SELECT c.grp, c.name, NULL, c.is_emergency, p.id, c.bearer, c.photo
FROM (VALUES
    ('Electrical',        'Power outage in one area',                    'high',      false, 'building', false),
    ('Electrical',        'Power outage in the whole building',          'emergency', true,  'building', false),
    ('Electrical',        'Socket or switch not working',                'normal',    false, 'building', false),
    ('Electrical',        'Breaker keeps tripping',                      'normal',    false, 'building', false),
    ('Electrical',        'Light not working',                           'low',       false, 'building', false),
    ('Water',             'Leaking pipe or tap',                         'normal',    false, 'building', true),
    ('Water',             'Burst pipe or flooding',                      'emergency', true,  'building', false),
    ('Water',             'Clogged drain',                               'normal',    false, 'building', false),
    ('Water',             'Toilet not flushing',                         'normal',    false, 'building', false),
    ('Air conditioning',  'Not cooling',                                 'normal',    false, 'building', false),
    ('Air conditioning',  'Water dripping from unit',                    'normal',    false, 'building', true),
    ('Elevator',          'Person stuck in elevator',                    'emergency', true,  'building', false),
    ('Elevator',          'Elevator out of service',                     'high',      false, 'building', false),
    ('Doors and locks',   'Door or lock broken',                         'normal',    false, 'building', false),
    ('Ceiling and walls', 'Water stain on ceiling',                      'low',       false, 'building', true),
    ('Ceiling and walls', 'Tenant fit-out damage (partition, ceiling)',  'low',       false, 'customer', false),
    ('Fire safety',       'False fire alarm',                            'high',      false, 'building', false)
) AS c(grp, name, priority, is_emergency, bearer, photo)
JOIN priority_levels p ON p.code = c.priority
ON CONFLICT (group_name, name) DO NOTHING;

-- ---------------------------------------------------------------------------
-- cases, case_status_history  (owner: Trí — F3)
-- Demo cases at different points of the road. Each case is built from the canonical F3 path up to its target status:
--   pending -> approved -> assigned -> accepted -> in_progress -> completed -> closed   ("rejected" branches off pending).
-- Each customer reports in their own room or a common area of their building (same rule as the API).
-- A case is skipped when one with the same title already exists, so the file can run again.
-- ---------------------------------------------------------------------------
DO $$
DECLARE
    r        record;
    path     text[] := ARRAY['pending', 'approved', 'assigned', 'accepted', 'in_progress', 'completed', 'closed'];
    n        int;
    i        int;
    cid      bigint;
    cat      record;
    rm       record;
    cust_id  bigint;
    stf_id   bigint;
    tech_id  bigint;
    tech_nm  text;
    ast_id   bigint;
    t0       timestamptz;
    t_start  timestamptz;
    t_done   timestamptz;
    t_close  timestamptz;
    step_at  timestamptz;
    who      bigint;
BEGIN
    FOR r IN
        SELECT * FROM (VALUES
            ('Air conditioner blows warm air', 'The AC in our office has been running since 8am but the room is still 30°C. No error light on the unit.',
             'Air conditioning', 'Not cooling', 'SmartMaint Tower A', '501', 'Split AC Daikin 2HP (#AC-501)', 'customer', NULL::text, 'pending', 2, NULL::int),
            ('Two lights out in corridor 5', 'The two ceiling lights near the fire exit are off. The corridor is dark at night.',
             'Electrical', 'Light not working', 'SmartMaint Tower A', 'Corridor 5', NULL::text, 'bao', NULL::text, 'pending', 5, NULL::int),
            ('Room 301 has no power', 'All sockets and lights in room 301 are off since this morning.',
             'Electrical', 'Power outage in one area', 'Riverside Office B', '301', NULL::text, 'hoa', NULL::text, 'pending', 1, NULL::int),
            ('Water leaking under the washbasin', 'Water is dripping from the pipe under the second washbasin, the floor is wet.',
             'Water', 'Leaking pipe or tap', 'SmartMaint Tower A', 'Restroom G', NULL::text, 'bao', NULL::text, 'approved', 7, NULL::int),
            ('Socket next to the printer is dead', 'The double socket on the left wall gives no power. Other sockets are fine.',
             'Electrical', 'Socket or switch not working', 'SmartMaint Tower A', '1201', NULL::text, 'lan', 'technician', 'assigned', 9, NULL::int),
            ('AC dripping water on the desk', 'Water drips from the indoor unit onto the desk below every few minutes.',
             'Air conditioning', 'Water dripping from unit', 'SmartMaint Tower A', '502', 'Split AC Panasonic 1.5HP (#AC-502)', 'bao', 'technician', 'in_progress', 22, 3),
            ('Toilet in restroom G keeps running', 'The toilet in the second stall does not stop filling after flushing.',
             'Water', 'Toilet not flushing', 'SmartMaint Tower A', 'Restroom G', NULL::text, 'bao', 'long', 'in_progress', 40, 30),
            ('Breaker trips when the kettle is on', 'The pantry breaker trips every time we use the kettle and the microwave together.',
             'Electrical', 'Breaker keeps tripping', 'SmartMaint Tower A', '1201', 'Distribution board DB-12 (#EL-12)', 'lan', 'duc', 'completed', 30, 8),
            ('Meeting room door does not lock', 'The key turns but the latch does not move.',
             'Doors and locks', 'Door or lock broken', 'SmartMaint Tower A', 'Meeting room 12', NULL::text, 'customer', 'long', 'closed', 120, 100),
            ('Ceiling panel flickering', 'The LED panel above my desk flickers all day.',
             'Electrical', 'Light not working', 'SmartMaint Tower A', '501', 'LED ceiling panel (#LT-501)', 'customer', 'technician', 'closed', 200, 180),
            ('Something is broken', 'Please check.',
             'Water', 'Clogged drain', 'SmartMaint Tower A', '501', NULL::text, 'customer', NULL::text, 'rejected', 90, NULL::int)
        ) AS t(title, descr, grp, typ, building, room_number, asset, cust_username, tech_username, target, submitted_h, started_h)
    LOOP
        CONTINUE WHEN EXISTS (SELECT 1 FROM cases WHERE title = r.title);

        SELECT c.id, c.default_priority_id, c.default_cost_bearer, p.sla_hours INTO cat
        FROM case_categories c JOIN priority_levels p ON p.id = c.default_priority_id
        WHERE c.group_name = r.grp AND c.name = r.typ;
        SELECT rooms.id, fl.building_id INTO rm
        FROM rooms JOIN floors fl ON fl.id = rooms.floor_id JOIN buildings b ON b.id = fl.building_id
        WHERE b.name = r.building AND rooms.room_number = r.room_number;
        SELECT id INTO cust_id FROM accounts WHERE username = r.cust_username;
        SELECT id, full_name INTO tech_id, tech_nm FROM accounts WHERE username = r.tech_username;
        SELECT id INTO ast_id FROM assets WHERE name = r.asset AND room_id = rm.id;
        SELECT staff_id INTO stf_id FROM staff_building_assignments WHERE building_id = rm.building_id ORDER BY is_primary DESC, staff_id LIMIT 1;

        n := CASE WHEN r.target = 'rejected' THEN 1 ELSE array_position(path, r.target) END;
        t0 := now() - make_interval(hours => r.submitted_h);
        t_start := CASE WHEN n >= 5 THEN now() - make_interval(hours => COALESCE(r.started_h, 3)) END;
        t_done := CASE WHEN n >= 6 THEN now() - make_interval(hours => GREATEST(COALESCE(r.started_h, 3) - 4, 1)) END;
        t_close := CASE WHEN n >= 7 THEN t_done + interval '2 hours' END;

        INSERT INTO cases (requesting_customer_id, room_id, asset_id, category_id, title, description, status_id, priority_level_id, cost_bearer,
                           staff_id, assigned_technician_id, submitted_at, started_at, due_at, completed_at, closed_at,
                           actual_completion_hours, root_cause, parts_used)
        VALUES (cust_id, rm.id, ast_id, cat.id, r.title, r.descr,
                (SELECT id FROM case_statuses WHERE name = r.target),
                cat.default_priority_id, cat.default_cost_bearer,
                CASE WHEN n >= 2 OR r.target = 'rejected' THEN stf_id END,
                CASE WHEN n >= 3 THEN tech_id END,
                t0, t_start, t_start + make_interval(hours => cat.sla_hours), t_done, t_close,
                CASE WHEN n >= 6 THEN 2.50 END,
                CASE WHEN n >= 6 THEN 'Overloaded circuit / worn part (demo data)' END,
                CASE WHEN n >= 6 THEN '1x replacement part, bought outside the system (demo data)' END)
        RETURNING id INTO cid;

        -- status history: one row per step of the path (who: customer, staff or technician; when: spread between submit and now)
        FOR i IN 1..n LOOP
            who := CASE WHEN i = 1 THEN cust_id WHEN i IN (2, 3, 6, 7) THEN stf_id ELSE tech_id END;
            step_at := CASE
                WHEN i = 1 THEN t0
                WHEN i = 5 THEN t_start
                WHEN i = 6 THEN t_done
                WHEN i = 7 THEN t_close
                ELSE t0 + make_interval(hours => i - 1) END;
            INSERT INTO case_status_history (case_id, new_status_id, changed_by_id, reason, changed_at)
            VALUES (cid, (SELECT id FROM case_statuses WHERE name = path[i]), who,
                    CASE WHEN i = 3 THEN 'Assigned to ' || tech_nm || '.' END, step_at);
        END LOOP;

        IF r.target = 'rejected' THEN
            INSERT INTO case_status_history (case_id, new_status_id, changed_by_id, reason, changed_at)
            VALUES (cid, (SELECT id FROM case_statuses WHERE name = 'rejected'), stf_id,
                    'Missing information: which drain, and where? Please add a photo.', t0 + interval '2 hours');
        END IF;
    END LOOP;
END
$$;
