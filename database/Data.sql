-- ============================================================================
-- SmartMaint — seed data (Data.sql)
-- Single source of seed data: run automatically by the API on local start
-- (Database__MigrateAndSeedOnStartup=true) and submitted with Report 4/5.
--
-- Rules:
--   * Idempotent: every INSERT ends with ON CONFLICT ... DO NOTHING, so the file can run any number of times.
--   * Never insert id values (identity ALWAYS); reference other rows by their natural key (email, code).
--   * One section per table, in FK order. Each flow owner appends the section for their own tables.
--   * DEMO DATA ONLY — no real people, no real passwords.
--
-- Demo accounts — all use the password:  Smartmaint@2026
--   admin@smartmaint.test        Administrator
--   manager@smartmaint.test      Facility Manager
--   officer@smartmaint.test      Facility Officer
--   technician@smartmaint.test   Maintenance Technician
--   customer@smartmaint.test     Customer
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
    ('officer@smartmaint.test',    'Demo Facility Officer', NULL, 'officer',   'active',
     'AQAAAAIAAYagAAAAEA0mgM6d1JJNIWBoFNFIEyvDF+GUDFklW9VZIoY6HP8tnqvsBq1HuTPy++blx3jRCg==', false, 0),
    ('technician@smartmaint.test', 'Demo Technician',       NULL, 'technician', 'active',
     'AQAAAAIAAYagAAAAEHZA3bhJRxkdFqUtQQ0KwuwElOGqJUqQP+W4QRZNS68j3k9jq4EZBwc/E7tmZwyeaA==', false, 0),
    ('customer@smartmaint.test',   'Demo Customer',         NULL, 'customer',  'active',
     'AQAAAAIAAYagAAAAEPsUi5sdT96poC9UdjfZNigi6qmwILSyfugb8LnNojE3ZI9BVv6AC/+XdMxCKtFg7w==', false, 0),
    ('newuser@smartmaint.test',    'Demo New Customer',     NULL, 'customer',  'active',
     'AQAAAAIAAYagAAAAEHiuxRI8CvJ7xMnmYzN7viZFj+kAQr6Lv1NcH4KM8pcKuiKPU4Nhyj44vcPci27zFQ==', true, 0)
ON CONFLICT (email) DO NOTHING;
