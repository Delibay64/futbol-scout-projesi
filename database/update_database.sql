-- ========================================
-- FUTBOL SCOUT PROJESİ - VERİTABANI GÜNCELLEME (MIGRATION)
-- ========================================
-- Bu script mevcut ScoutDB veritabanına yeni Stored Procedure'leri,
-- Index'leri ve Constraint'leri ekler.
-- ========================================

\echo '==================================='
\echo 'FUTBOL SCOUT - VERITABANI GÜNCELLEME'
\echo '==================================='

-- 1. YENİ STORED PROCEDURE'LER

\echo ''
\echo '1. Stored Procedure''ler ekleniyor...'

-- SP 2: Scout Raporu Oluştur
CREATE OR REPLACE PROCEDURE sp_CreateScoutReport(
    p_user_id INT,
    p_player_id INT,
    p_predicted_value DECIMAL,
    p_notes TEXT
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO scoutreports (user_id, player_id, predicted_value, notes, report_date)
    VALUES (p_user_id, p_player_id, p_predicted_value, p_notes, CURRENT_TIMESTAMP);

    RAISE NOTICE 'Scout raporu oluşturuldu: Oyuncu %, Kullanıcı %, Tahmin %', p_player_id, p_user_id, p_predicted_value;
END;
$$;

\echo '   ✓ sp_CreateScoutReport oluşturuldu'

-- SP 3: Oyuncu İstatistiği Güncelle
CREATE OR REPLACE PROCEDURE sp_UpdatePlayerStats(
    p_player_id INT,
    p_season VARCHAR(20),
    p_matches INT,
    p_goals INT,
    p_assists INT,
    p_yellow_cards INT DEFAULT 0,
    p_red_cards INT DEFAULT 0,
    p_minutes INT DEFAULT 0
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_stat_id INT;
BEGIN
    SELECT stat_id INTO v_stat_id
    FROM playerstats
    WHERE player_id = p_player_id AND season = p_season;

    IF v_stat_id IS NOT NULL THEN
        UPDATE playerstats
        SET matches_played = p_matches,
            goals = p_goals,
            assists = p_assists,
            yellow_cards = p_yellow_cards,
            red_cards = p_red_cards,
            minutes_played = p_minutes
        WHERE stat_id = v_stat_id;
        RAISE NOTICE 'Oyuncu % için % sezonu istatistikleri güncellendi', p_player_id, p_season;
    ELSE
        INSERT INTO playerstats (player_id, season, matches_played, goals, assists, yellow_cards, red_cards, minutes_played)
        VALUES (p_player_id, p_season, p_matches, p_goals, p_assists, p_yellow_cards, p_red_cards, p_minutes);
        RAISE NOTICE 'Oyuncu % için % sezonu istatistikleri eklendi', p_player_id, p_season;
    END IF;
END;
$$;

\echo '   ✓ sp_UpdatePlayerStats oluşturuldu'

-- 2. INDEX'LER

\echo ''
\echo '2. Index''ler oluşturuluyor...'

-- Foreign Key Index'leri
CREATE INDEX IF NOT EXISTS idx_players_team_id ON players(team_id);
CREATE INDEX IF NOT EXISTS idx_playerstats_player_id ON playerstats(player_id);
CREATE INDEX IF NOT EXISTS idx_scoutreports_player_id ON scoutreports(player_id);
CREATE INDEX IF NOT EXISTS idx_scoutreports_user_id ON scoutreports(user_id);
CREATE INDEX IF NOT EXISTS idx_users_role_id ON users(role_id);
CREATE INDEX IF NOT EXISTS idx_player_price_log_player_id ON player_price_log(player_id);

\echo '   ✓ 6 Foreign Key index oluşturuldu'

-- Filtreleme Index'leri
CREATE INDEX IF NOT EXISTS idx_players_age ON players(age);
CREATE INDEX IF NOT EXISTS idx_players_position ON players(position);
CREATE INDEX IF NOT EXISTS idx_playerstats_season ON playerstats(season);

\echo '   ✓ 3 Filtreleme index oluşturuldu'

-- Composite Index'ler
CREATE INDEX IF NOT EXISTS idx_playerstats_player_season ON playerstats(player_id, season);
CREATE INDEX IF NOT EXISTS idx_scoutreports_date_desc ON scoutreports(report_date DESC);

\echo '   ✓ 2 Composite index oluşturuldu'

-- 3. CHECK CONSTRAINT'LERİ

\echo ''
\echo '3. CHECK Constraint''leri ekleniyor...'

-- Mevcut constraint'leri kontrol et ve varsa ekleme
DO $$
BEGIN
    -- Players tablosu constraint'leri
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'chk_players_age') THEN
        ALTER TABLE players ADD CONSTRAINT chk_players_age CHECK (age > 0 AND age < 100);
        RAISE NOTICE '   ✓ chk_players_age eklendi';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'chk_players_value') THEN
        ALTER TABLE players ADD CONSTRAINT chk_players_value CHECK (current_market_value >= 0);
        RAISE NOTICE '   ✓ chk_players_value eklendi';
    END IF;

    -- Playerstats tablosu constraint'leri
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'chk_playerstats_matches') THEN
        ALTER TABLE playerstats ADD CONSTRAINT chk_playerstats_matches CHECK (matches_played >= 0);
        RAISE NOTICE '   ✓ chk_playerstats_matches eklendi';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'chk_playerstats_goals') THEN
        ALTER TABLE playerstats ADD CONSTRAINT chk_playerstats_goals CHECK (goals >= 0);
        RAISE NOTICE '   ✓ chk_playerstats_goals eklendi';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'chk_playerstats_assists') THEN
        ALTER TABLE playerstats ADD CONSTRAINT chk_playerstats_assists CHECK (assists >= 0);
        RAISE NOTICE '   ✓ chk_playerstats_assists eklendi';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'chk_playerstats_cards') THEN
        ALTER TABLE playerstats ADD CONSTRAINT chk_playerstats_cards CHECK (yellow_cards >= 0 AND red_cards >= 0);
        RAISE NOTICE '   ✓ chk_playerstats_cards eklendi';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'chk_playerstats_minutes') THEN
        ALTER TABLE playerstats ADD CONSTRAINT chk_playerstats_minutes CHECK (minutes_played >= 0);
        RAISE NOTICE '   ✓ chk_playerstats_minutes eklendi';
    END IF;

    -- Scoutreports tablosu constraint'i
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'chk_scoutreports_value') THEN
        ALTER TABLE scoutreports ADD CONSTRAINT chk_scoutreports_value CHECK (predicted_value >= 0);
        RAISE NOTICE '   ✓ chk_scoutreports_value eklendi';
    END IF;
END $$;

-- 4. DOĞRULAMA

\echo ''
\echo '==================================='
\echo 'GÜNCELLEME TAMAMLANDI!'
\echo '==================================='
\echo ''
\echo 'Doğrulama:'
\echo ''

-- Stored Procedure sayısını kontrol et
SELECT COUNT(*) AS stored_procedure_count
FROM pg_proc
WHERE proname LIKE 'sp_%'
AND pg_get_function_result(oid) = 'void';

\echo ''

-- Index sayısını kontrol et
SELECT COUNT(*) AS index_count
FROM pg_indexes
WHERE schemaname = 'public'
AND indexname LIKE 'idx_%';

\echo ''

-- Constraint sayısını kontrol et
SELECT COUNT(*) AS check_constraint_count
FROM pg_constraint
WHERE conname LIKE 'chk_%';

\echo ''
\echo 'Güncelleme başarılı! Artık:'
\echo '  - 3 Stored Procedure var'
\echo '  - 11 Performans Index var'
\echo '  - 8 CHECK Constraint var'
\echo ''
