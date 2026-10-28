-- ============================================================================
-- SmartMaint — seed data (Data.sql)
-- Single source of seed data: run automatically by the API on local start
-- (Database__MigrateAndSeedOnStartup=true) and submitted with Report 4/5.
--
-- Rules:
--   * Idempotent: every INSERT ends with ON CONFLICT ... DO NOTHING (or is guarded), so the file can run any number of times.
--   * Never insert id values (identity ALWAYS); reference other rows by their natural key (email, name, code).
--   * One section per table, in FK order. Each flow owner appends the section for their own tables.
--   * DEMO DATA ONLY — no real people, no real passwords.
--
-- Demo accounts — all use the password:  Smartmaint@2026
--   admin@smartmaint.test        Administrator
--   manager@smartmaint.test      Facility Manager (both buildings)
--   staff@smartmaint.test        Facility Staff, primary for SmartMaint Tower A
--   tuan@smartmaint.test         Facility Staff, primary for Riverside Office B, backup for Tower A
--   technician@smartmaint.test   Maintenance Technician
--   duc@smartmaint.test          Maintenance Technician
--   long@smartmaint.test         Maintenance Technician
--   customer@smartmaint.test     Customer
--   bao@smartmaint.test          Customer
--   hoa@smartmaint.test          Customer (Riverside Office B)
--   newuser@smartmaint.test      Customer with must_change_password = true (to demo AUTH-04)
-- ============================================================================

-- ---------------------------------------------------------------------------
-- users  (owner: Trí — core)
-- password_hash = ASP.NET Core Identity v3 (PBKDF2-HMAC-SHA512, 100 000 iterations)
-- ---------------------------------------------------------------------------
INSERT INTO users (email, full_name, phone, role, status, password_hash, must_change_password, failed_login_count)
VALUES
    ('admin@smartmaint.test',      'System Administrator', NULL, 'admin',      'active',
     'AQAAAAIAAYagAAAAECzsqwdj++qsn/kHW6W2sDWOqw+6uEdg6WhjSwdlhYf0yZJONrgOnyikQjbVPluNOg==', false, 0),
    ('manager@smartmaint.test',    'Demo Facility Manager', NULL, 'manager',   'active',
     'AQAAAAIAAYagAAAAEKJ0YOEksCjFB/AuvGDzBw9cDZV6r+Aw6TQ5H3IXY6rRK/ib0dJAM2F3S/E6ZVTCcA==', false, 0),
    ('staff@smartmaint.test',      'Demo Facility Staff',   NULL, 'staff',     'active',
     'AQAAAAIAAYagAAAAEA0mgM6d1JJNIWBoFNFIEyvDF+GUDFklW9VZIoY6HP8tnqvsBq1HuTPy++blx3jRCg==', false, 0),
    ('tuan@smartmaint.test',       'Nguyen Van Tuan',       NULL, 'staff',     'active',
     'AQAAAAIAAYagAAAAEA0mgM6d1JJNIWBoFNFIEyvDF+GUDFklW9VZIoY6HP8tnqvsBq1HuTPy++blx3jRCg==', false, 0),
    ('technician@smartmaint.test', 'Demo Technician',       NULL, 'technician', 'active',
     'AQAAAAIAAYagAAAAEHZA3bhJRxkdFqUtQQ0KwuwElOGqJUqQP+W4QRZNS68j3k9jq4EZBwc/E7tmZwyeaA==', false, 0),
    ('duc@smartmaint.test',        'Pham Minh Duc',         NULL, 'technician', 'active',
     'AQAAAAIAAYagAAAAEHZA3bhJRxkdFqUtQQ0KwuwElOGqJUqQP+W4QRZNS68j3k9jq4EZBwc/E7tmZwyeaA==', false, 0),
    ('long@smartmaint.test',       'Vo Thanh Long',         NULL, 'technician', 'active',
     'AQAAAAIAAYagAAAAEHZA3bhJRxkdFqUtQQ0KwuwElOGqJUqQP+W4QRZNS68j3k9jq4EZBwc/E7tmZwyeaA==', false, 0),
    ('customer@smartmaint.test',   'Demo Customer',         NULL, 'customer',  'active',
     'AQAAAAIAAYagAAAAEPsUi5sdT96poC9UdjfZNigi6qmwILSyfugb8LnNojE3ZI9BVv6AC/+XdMxCKtFg7w==', false, 0),
    ('bao@smartmaint.test',        'Tran Quoc Bao',         NULL, 'customer',  'active',
     'AQAAAAIAAYagAAAAEPsUi5sdT96poC9UdjfZNigi6qmwILSyfugb8LnNojE3ZI9BVv6AC/+XdMxCKtFg7w==', false, 0),
    ('hoa@smartmaint.test',        'Le Thi Hoa',            NULL, 'customer',  'active',
     'AQAAAAIAAYagAAAAEPsUi5sdT96poC9UdjfZNigi6qmwILSyfugb8LnNojE3ZI9BVv6AC/+XdMxCKtFg7w==', false, 0),
    ('newuser@smartmaint.test',    'Demo New Customer',     NULL, 'customer',  'active',
     'AQAAAAIAAYagAAAAEHiuxRI8CvJ7xMnmYzN7viZFj+kAQr6Lv1NcH4KM8pcKuiKPU4Nhyj44vcPci27zFQ==', true, 0)
ON CONFLICT (email) DO NOTHING;

-- ---------------------------------------------------------------------------
-- buildings, floors, rooms, assets  (owner: Huy — F1; minimal set so F3 can run)
-- ---------------------------------------------------------------------------
INSERT INTO buildings (name, address)
VALUES ('SmartMaint Tower A', '12 Nguyen Hue, District 1'),
       ('Riverside Office B', '88 Ton Duc Thang, District 1')
ON CONFLICT (name) DO NOTHING;

INSERT INTO floors (building_id, name)
SELECT b.id, f.name
FROM (VALUES ('SmartMaint Tower A', 'Ground floor'),
             ('SmartMaint Tower A', 'Floor 5'),
             ('SmartMaint Tower A', 'Floor 12'),
             ('Riverside Office B', 'Floor 3')) AS f(building, name)
JOIN buildings b ON b.name = f.building
ON CONFLICT (building_id, name) DO NOTHING;

INSERT INTO rooms (floor_id, name)
SELECT fl.id, r.name
FROM (VALUES ('SmartMaint Tower A', 'Ground floor', 'Main lobby'),
             ('SmartMaint Tower A', 'Ground floor', 'Restroom G'),
             ('SmartMaint Tower A', 'Floor 5',      'Room 501 · An Phat Co.'),
             ('SmartMaint Tower A', 'Floor 5',      'Room 502 · Lumen Studio'),
             ('SmartMaint Tower A', 'Floor 5',      'Corridor 5'),
             ('SmartMaint Tower A', 'Floor 12',     'Room 1201 · Vista Law'),
             ('SmartMaint Tower A', 'Floor 12',     'Meeting room 12'),
             ('Riverside Office B', 'Floor 3',      'Room 301 · Minh Long JSC'),
             ('Riverside Office B', 'Floor 3',      'Room 302')) AS r(building, floor, name)
JOIN buildings b ON b.name = r.building
JOIN floors fl ON fl.building_id = b.id AND fl.name = r.floor
ON CONFLICT (floor_id, name) DO NOTHING;

INSERT INTO assets (room_id, name)
SELECT rm.id, a.name
FROM (VALUES ('Room 501 · An Phat Co.',  'Split AC Daikin 2HP (#AC-501)'),
             ('Room 501 · An Phat Co.',  'LED ceiling panel (#LT-501)'),
             ('Room 502 · Lumen Studio', 'Split AC Panasonic 1.5HP (#AC-502)'),
             ('Restroom G',              'Water heater Ariston 30L (#WH-G1)'),
             ('Room 1201 · Vista Law',   'Distribution board DB-12 (#EL-12)'),
             ('Main lobby',              'Passenger elevator 1 (#EV-01)')) AS a(room, name)
JOIN rooms rm ON rm.name = a.room
ON CONFLICT (room_id, name) DO NOTHING;

-- ---------------------------------------------------------------------------
-- staff_building_assignments, manager_building_assignments  (owner: Huy — F1)
-- A building has several staff; is_primary marks the main one.
-- ---------------------------------------------------------------------------
INSERT INTO staff_building_assignments (staff_id, building_id, is_primary)
SELECT u.id, b.id, a.is_primary
FROM (VALUES ('staff@smartmaint.test', 'SmartMaint Tower A', true),
             ('tuan@smartmaint.test',  'Riverside Office B', true),
             ('tuan@smartmaint.test',  'SmartMaint Tower A', false)) AS a(email, building, is_primary)
JOIN users u ON u.email = a.email
JOIN buildings b ON b.name = a.building
ON CONFLICT (staff_id, building_id) DO NOTHING;

INSERT INTO manager_building_assignments (manager_id, building_id)
SELECT u.id, b.id
FROM users u CROSS JOIN buildings b
WHERE u.email = 'manager@smartmaint.test'
ON CONFLICT (manager_id, building_id) DO NOTHING;

-- ---------------------------------------------------------------------------
-- priority_levels, case_categories  (owner: Trí — F3 core; categories come from F1 later)
-- sla_hours is wall-clock for now (office-hour counting: BR-08, O04).
-- ---------------------------------------------------------------------------
INSERT INTO priority_levels (code, name, sla_hours)
VALUES ('emergency', 'Emergency', 2),
       ('high',      'High',      8),
       ('normal',    'Normal',   24),
       ('low',       'Low',      72)
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
-- cases, case_status_history, case_assignments  (owner: Trí — F3)
-- Demo cases at different points of the road. Each case is built from the canonical F3 path up to its target status:
--   pending -> approved -> assigned -> accepted -> in_progress -> completed -> closed   ("rejected" branches off pending).
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
             'Air conditioning', 'Not cooling', 'Room 501 · An Phat Co.', 'Split AC Daikin 2HP (#AC-501)', 'customer@smartmaint.test', NULL::text, 'pending', 2, NULL::int),
            ('Two lights out in corridor 5', 'The two ceiling lights near the fire exit are off. The corridor is dark at night.',
             'Electrical', 'Light not working', 'Corridor 5', NULL::text, 'bao@smartmaint.test', NULL::text, 'pending', 5, NULL::int),
            ('Room 301 has no power', 'All sockets and lights in room 301 are off since this morning.',
             'Electrical', 'Power outage in one area', 'Room 301 · Minh Long JSC', NULL::text, 'hoa@smartmaint.test', NULL::text, 'pending', 1, NULL::int),
            ('Water leaking under the washbasin', 'Water is dripping from the pipe under the second washbasin, the floor is wet.',
             'Water', 'Leaking pipe or tap', 'Restroom G', NULL::text, 'bao@smartmaint.test', NULL::text, 'approved', 7, NULL::int),
            ('Socket next to the printer is dead', 'The double socket on the left wall gives no power. Other sockets are fine.',
             'Electrical', 'Socket or switch not working', 'Room 1201 · Vista Law', NULL::text, 'customer@smartmaint.test', 'technician@smartmaint.test', 'assigned', 9, NULL::int),
            ('AC dripping water on the desk', 'Water drips from the indoor unit onto the desk below every few minutes.',
             'Air conditioning', 'Water dripping from unit', 'Room 502 · Lumen Studio', 'Split AC Panasonic 1.5HP (#AC-502)', 'customer@smartmaint.test', 'technician@smartmaint.test', 'in_progress', 22, 3),
            ('Toilet in restroom G keeps running', 'The toilet in the second stall does not stop filling after flushing.',
             'Water', 'Toilet not flushing', 'Restroom G', NULL::text, 'bao@smartmaint.test', 'long@smartmaint.test', 'in_progress', 40, 30),
            ('Breaker trips when the kettle is on', 'The pantry breaker trips every time we use the kettle and the microwave together.',
             'Electrical', 'Breaker keeps tripping', 'Room 1201 · Vista Law', 'Distribution board DB-12 (#EL-12)', 'customer@smartmaint.test', 'duc@smartmaint.test', 'completed', 30, 8),
            ('Meeting room door does not lock', 'The key turns but the latch does not move.',
             'Doors and locks', 'Door or lock broken', 'Meeting room 12', NULL::text, 'customer@smartmaint.test', 'long@smartmaint.test', 'closed', 120, 100),
            ('Ceiling panel flickering', 'The LED panel above my desk flickers all day.',
             'Electrical', 'Light not working', 'Room 501 · An Phat Co.', 'LED ceiling panel (#LT-501)', 'customer@smartmaint.test', 'technician@smartmaint.test', 'closed', 200, 180),
            ('Something is broken', 'Please check.',
             'Water', 'Clogged drain', 'Room 501 · An Phat Co.', NULL::text, 'customer@smartmaint.test', NULL::text, 'rejected', 90, NULL::int)
        ) AS t(title, descr, grp, typ, room, asset, cust_email, tech_email, target, submitted_h, started_h)
    LOOP
        CONTINUE WHEN EXISTS (SELECT 1 FROM cases WHERE title = r.title);

        SELECT c.id, c.default_priority_id, c.default_cost_bearer, p.sla_hours INTO cat
        FROM case_categories c JOIN priority_levels p ON p.id = c.default_priority_id
        WHERE c.group_name = r.grp AND c.name = r.typ;
        SELECT rooms.id, fl.building_id INTO rm
        FROM rooms JOIN floors fl ON fl.id = rooms.floor_id WHERE rooms.name = r.room;
        SELECT id INTO cust_id FROM users WHERE email = r.cust_email;
        SELECT id INTO tech_id FROM users WHERE email = r.tech_email;
        SELECT id INTO ast_id FROM assets WHERE name = r.asset AND room_id = rm.id;
        SELECT staff_id INTO stf_id FROM staff_building_assignments WHERE building_id = rm.building_id ORDER BY is_primary DESC, staff_id LIMIT 1;

        n := CASE WHEN r.target = 'rejected' THEN 1 ELSE array_position(path, r.target) END;
        t0 := now() - make_interval(hours => r.submitted_h);
        t_start := CASE WHEN n >= 5 THEN now() - make_interval(hours => COALESCE(r.started_h, 3)) END;
        t_done := CASE WHEN n >= 6 THEN now() - make_interval(hours => GREATEST(COALESCE(r.started_h, 3) - 4, 1)) END;
        t_close := CASE WHEN n >= 7 THEN t_done + interval '2 hours' END;

        INSERT INTO cases (requesting_customer_id, room_id, asset_id, category_id, title, description, status, priority_level_id, cost_bearer,
                           staff_id, assigned_technician_id, submitted_at, started_at, due_at, completed_at, closed_at,
                           actual_hours, root_cause, parts_used)
        VALUES (cust_id, rm.id, ast_id, cat.id, r.title, r.descr, r.target, cat.default_priority_id, cat.default_cost_bearer,
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
            INSERT INTO case_status_history (case_id, from_status, to_status, changed_by_id, reason, changed_at)
            VALUES (cid, CASE WHEN i > 1 THEN path[i - 1] END, path[i], who, NULL, step_at);
        END LOOP;

        IF r.target = 'rejected' THEN
            INSERT INTO case_status_history (case_id, from_status, to_status, changed_by_id, reason, changed_at)
            VALUES (cid, 'pending', 'rejected', stf_id,
                    'Missing information: which drain, and where? Please add a photo.', t0 + interval '2 hours');
        END IF;

        IF n >= 3 THEN
            INSERT INTO case_assignments (case_id, technician_id, assigned_by_id, assigned_at, response, responded_at)
            VALUES (cid, tech_id, stf_id, t0 + interval '2 hours',
                    CASE WHEN n >= 4 THEN 'accepted' ELSE 'waiting' END,
                    CASE WHEN n >= 4 THEN t0 + interval '3 hours' END);
        END IF;
    END LOOP;
END
$$;
