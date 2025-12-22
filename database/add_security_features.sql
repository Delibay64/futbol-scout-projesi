-- ========================================
-- GÜVENLİK ÖZELLİKLERİ: YETKİLENDİRME VE MASKELEME
-- ========================================

\echo '======================================='
\echo 'GÜVENLİK ÖZELLİKLERİ EKLENIYOR...'
\echo '======================================='

-- 1. PostgreSQL ROL YETKİLENDİRMESİ

\echo ''
\echo '1. PostgreSQL Rolleri oluşturuluyor...'

-- Admin rolü (Tam yetkili)
CREATE ROLE scoutdb_admin WITH LOGIN PASSWORD 'admin_secure_pass_2024';
GRANT ALL PRIVILEGES ON DATABASE ScoutDB TO scoutdb_admin;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO scoutdb_admin;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO scoutdb_admin;
\echo '   ✓ scoutdb_admin rolü oluşturuldu (Tam yetki)'

-- Scout rolü (Okuma + Scout raporu yazma)
CREATE ROLE scoutdb_scout WITH LOGIN PASSWORD 'scout_secure_pass_2024';
GRANT CONNECT ON DATABASE ScoutDB TO scoutdb_scout;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO scoutdb_scout;
GRANT INSERT, UPDATE ON scoutreports TO scoutdb_scout;
GRANT USAGE ON SEQUENCE scoutreports_report_id_seq TO scoutdb_scout;
\echo '   ✓ scoutdb_scout rolü oluşturuldu (Okuma + Scout raporu)'

-- Viewer rolü (Sadece okuma)
CREATE ROLE scoutdb_viewer WITH LOGIN PASSWORD 'viewer_secure_pass_2024';
GRANT CONNECT ON DATABASE ScoutDB TO scoutdb_viewer;
GRANT SELECT ON players, teams, playerstats TO scoutdb_viewer;
GRANT SELECT ON vw_playerdetailstr, vw_topscorers, vw_youngtalents, vw_teamsummary TO scoutdb_viewer;
\echo '   ✓ scoutdb_viewer rolü oluşturuldu (Sadece okuma)'

-- 2. VERİ MASKELEME (PostgreSQL Views ile)

\echo ''
\echo '2. Veri maskeleme view''ları oluşturuluyor...'

-- Kullanıcı bilgilerini maskeleyen view
CREATE OR REPLACE VIEW vw_users_masked AS
SELECT
    user_id,
    username,
    -- Email maskeleme: og****@gmail.com
    CASE
        WHEN email IS NOT NULL THEN
            SUBSTRING(email FROM 1 FOR 2) || '****' || SUBSTRING(email FROM POSITION('@' IN email))
        ELSE NULL
    END AS email_masked,
    role_id,
    created_at
FROM users;

\echo '   ✓ vw_users_masked oluşturuldu (Email maskeleme)'

-- Hassas oyuncu bilgilerini maskeleyen view
CREATE OR REPLACE VIEW vw_players_public AS
SELECT
    player_id,
    full_name,
    age,
    position,
    -- Milliyeti ilk 3 harf (TÜR**, ARJ**)
    SUBSTRING(nationality FROM 1 FOR 3) || '**' AS nationality_masked,
    team_id,
    -- Piyasa değerini yuvarla (hassas bilgi gizle)
    ROUND(current_market_value / 100000) * 100000 AS approx_market_value
FROM players;

\echo '   ✓ vw_players_public oluşturuldu (Milliyet ve değer maskeleme)'

-- Scout raporlarını maskeleyen view (notları kısalt)
CREATE OR REPLACE VIEW vw_scoutreports_summary AS
SELECT
    report_id,
    user_id,
    player_id,
    predicted_value,
    -- Notların ilk 50 karakteri (tam metni gizle)
    CASE
        WHEN LENGTH(notes) > 50 THEN SUBSTRING(notes FROM 1 FOR 50) || '...'
        ELSE notes
    END AS notes_summary,
    report_date
FROM scoutreports;

\echo '   ✓ vw_scoutreports_summary oluşturuldu (Not maskeleme)'

-- 3. ROW-LEVEL SECURITY (RLS) Politikaları

\echo ''
\echo '3. Row-Level Security politikaları ekleniyor...'

-- Scoutreports tablosuna RLS ekle
ALTER TABLE scoutreports ENABLE ROW LEVEL SECURITY;

-- Politika: Kullanıcılar sadece kendi raporlarını görebilir
CREATE POLICY scoutreports_user_policy ON scoutreports
    FOR SELECT
    USING (user_id = current_setting('app.current_user_id', TRUE)::INTEGER);

-- Politika: Admin her şeyi görebilir
CREATE POLICY scoutreports_admin_policy ON scoutreports
    FOR ALL
    USING (current_setting('app.user_role', TRUE) = 'Admin');

\echo '   ✓ Row-Level Security aktif'

-- 4. AUDIT LOG TABLOSU (Kim ne yaptı?)

\echo ''
\echo '4. Audit log tablosu oluşturuluyor...'

CREATE TABLE IF NOT EXISTS audit_logs (
    log_id SERIAL PRIMARY KEY,
    user_id INT,
    username VARCHAR(50),
    action_type VARCHAR(50), -- INSERT, UPDATE, DELETE, LOGIN
    table_name VARCHAR(50),
    record_id INT,
    old_value TEXT,
    new_value TEXT,
    ip_address VARCHAR(45),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

\echo '   ✓ audit_logs tablosu oluşturuldu'

-- Audit log için trigger fonksiyonu
CREATE OR REPLACE FUNCTION fn_audit_log()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        INSERT INTO audit_logs (action_type, table_name, record_id, new_value, username)
        VALUES ('INSERT', TG_TABLE_NAME, NEW.player_id, row_to_json(NEW)::TEXT, current_user);
        RETURN NEW;
    ELSIF TG_OP = 'UPDATE' THEN
        INSERT INTO audit_logs (action_type, table_name, record_id, old_value, new_value, username)
        VALUES ('UPDATE', TG_TABLE_NAME, OLD.player_id, row_to_json(OLD)::TEXT, row_to_json(NEW)::TEXT, current_user);
        RETURN NEW;
    ELSIF TG_OP = 'DELETE' THEN
        INSERT INTO audit_logs (action_type, table_name, record_id, old_value, username)
        VALUES ('DELETE', TG_TABLE_NAME, OLD.player_id, row_to_json(OLD)::TEXT, current_user);
        RETURN OLD;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Players tablosuna audit trigger ekle
CREATE TRIGGER trg_players_audit
AFTER INSERT OR UPDATE OR DELETE ON players
FOR EACH ROW EXECUTE FUNCTION fn_audit_log();

\echo '   ✓ Audit trigger eklendi'

-- 5. VERİ ERİŞİM KONTROL FONKSİYONU

CREATE OR REPLACE FUNCTION fn_check_user_permission(
    p_user_role VARCHAR,
    p_required_role VARCHAR
)
RETURNS BOOLEAN AS $$
BEGIN
    IF p_user_role = 'Admin' THEN
        RETURN TRUE; -- Admin her şeye erişebilir
    ELSIF p_user_role = p_required_role THEN
        RETURN TRUE;
    ELSE
        RETURN FALSE;
    END IF;
END;
$$ LANGUAGE plpgsql;

\echo '   ✓ Yetkilendirme fonksiyonu oluşturuldu'

-- 6. DOĞRULAMA

\echo ''
\echo '======================================='
\echo 'GÜVENLİK ÖZELLİKLERİ TAMAMLANDI!'
\echo '======================================='
\echo ''
\echo 'Eklenen Özellikler:'
\echo '  ✓ 3 PostgreSQL Rolü (admin, scout, viewer)'
\echo '  ✓ 3 Maskeleme View''ı (users, players, scoutreports)'
\echo '  ✓ Row-Level Security (RLS) politikaları'
\echo '  ✓ Audit Log sistemi (Kim ne yaptı?)'
\echo '  ✓ Yetkilendirme fonksiyonu'
\echo ''
\echo 'Test Komutları:'
\echo '  SELECT * FROM vw_users_masked;'
\echo '  SELECT * FROM vw_players_public;'
\echo '  SELECT * FROM audit_logs;'
\echo ''
