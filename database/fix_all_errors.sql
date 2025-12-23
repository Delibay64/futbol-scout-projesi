-- TÜM HATALARI DÜZELTEN SQL SCRIPT
-- PostgreSQL'de çalıştırın: psql -U postgres -d scoutdb -f fix_all_errors.sql

-- ============================================
-- 1. SCOUTREPORTS TABLOSUNA is_approved EKLE
-- ============================================
ALTER TABLE scoutreports
ADD COLUMN IF NOT EXISTS is_approved BOOLEAN DEFAULT FALSE;

UPDATE scoutreports
SET is_approved = FALSE
WHERE is_approved IS NULL;

SELECT 'is_approved kolonu eklendi' AS sonuc;

-- ============================================
-- 2. STORED PROCEDURE: sp_CreateScoutReport
-- ============================================
DROP PROCEDURE IF EXISTS sp_CreateScoutReport(INT, INT, NUMERIC, TEXT);

CREATE OR REPLACE PROCEDURE sp_CreateScoutReport(
    p_user_id INT,
    p_player_id INT,
    p_predicted_value DECIMAL,
    p_notes TEXT
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Scout raporu ekle
    INSERT INTO scoutreports (user_id, player_id, predicted_value, notes, report_date, is_approved)
    VALUES (p_user_id, p_player_id, p_predicted_value, p_notes, CURRENT_TIMESTAMP, FALSE);

    RAISE NOTICE 'Scout raporu oluşturuldu: Oyuncu %, Kullanıcı %, Tahmin %', p_player_id, p_user_id, p_predicted_value;
END;
$$;

SELECT 'sp_CreateScoutReport procedure oluşturuldu' AS sonuc;

-- ============================================
-- 3. STORED PROCEDURE: sp_UpdateValue
-- ============================================
DROP PROCEDURE IF EXISTS sp_UpdateValue(INT, INT);

CREATE OR REPLACE PROCEDURE sp_UpdateValue(p_player_id INT, p_percentage INT)
LANGUAGE plpgsql
AS $$
DECLARE
    v_old_value DECIMAL(15, 2);
    v_new_value DECIMAL(15, 2);
BEGIN
    -- Mevcut değeri al
    SELECT current_market_value INTO v_old_value
    FROM players
    WHERE player_id = p_player_id;

    -- Yeni değeri hesapla
    v_new_value := v_old_value + (v_old_value * p_percentage / 100.0);

    -- Oyuncunun değerini güncelle
    UPDATE players
    SET current_market_value = v_new_value
    WHERE player_id = p_player_id;

    -- Log tablosuna kaydet
    INSERT INTO player_price_log (player_id, old_value, new_value, change_percentage, changed_at)
    VALUES (p_player_id, v_old_value, v_new_value, p_percentage, CURRENT_TIMESTAMP);

    RAISE NOTICE 'Oyuncu % değeri güncellendi: % -> % (%% %)', p_player_id, v_old_value, v_new_value, p_percentage;
END;
$$;

SELECT 'sp_UpdateValue procedure oluşturuldu' AS sonuc;

-- ============================================
-- 4. STORED PROCEDURE: sp_UpdatePlayerStats
-- ============================================
DROP PROCEDURE IF EXISTS sp_UpdatePlayerStats(INT, VARCHAR, INT, INT);

CREATE OR REPLACE PROCEDURE sp_UpdatePlayerStats(
    p_player_id INT,
    p_season VARCHAR,
    p_goals INT,
    p_assists INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Eğer sezon kaydı varsa UPDATE, yoksa INSERT
    IF EXISTS (
        SELECT 1 FROM playerstats
        WHERE player_id = p_player_id AND season = p_season
    ) THEN
        UPDATE playerstats
        SET goals = p_goals,
            assists = p_assists
        WHERE player_id = p_player_id AND season = p_season;
    ELSE
        INSERT INTO playerstats (player_id, season, goals, assists, matches_played, yellow_cards, red_cards, minutes_played)
        VALUES (p_player_id, p_season, p_goals, p_assists, 0, 0, 0, 0);
    END IF;

    RAISE NOTICE 'Oyuncu % için % sezonu istatistikleri güncellendi', p_player_id, p_season;
END;
$$;

SELECT 'sp_UpdatePlayerStats procedure oluşturuldu' AS sonuc;

-- ============================================
-- 5. KONTROL: Tüm procedure'leri listele
-- ============================================
SELECT
    proname AS procedure_name,
    pg_get_function_arguments(oid) AS parameters
FROM pg_proc
WHERE proname LIKE 'sp_%'
ORDER BY proname;

SELECT '✓ Tüm hatalar düzeltildi!' AS sonuc;
