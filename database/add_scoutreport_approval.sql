-- Scout Report Onay Sistemi
-- is_approved sütunu eklenir (default: false)
-- Sadece admin onayladığında true olur

\echo '========================================='
\echo 'SCOUT REPORT ONAY SİSTEMİ KURULUMU'
\echo '========================================='

-- 1. is_approved sütunu ekle
\echo ''
\echo '1. scoutreports tablosuna is_approved sütunu ekleniyor...'

ALTER TABLE scoutreports
ADD COLUMN IF NOT EXISTS is_approved BOOLEAN DEFAULT FALSE;

\echo '   ✓ is_approved sütunu eklendi (default: false)'

-- 2. Mevcut tüm raporları onayla (geçmişte eklenenler için)
\echo ''
\echo '2. Mevcut raporlar onaylanıyor...'

UPDATE scoutreports
SET is_approved = TRUE
WHERE is_approved IS NULL OR is_approved = FALSE;

\echo '   ✓ Mevcut tüm raporlar onaylandı'

-- 3. Stored procedure güncelle
\echo ''
\echo '3. sp_CreateScoutReport stored procedure güncelleniyor...'

CREATE OR REPLACE PROCEDURE sp_CreateScoutReport(
    p_user_id INT,
    p_player_id INT,
    p_predicted_value DECIMAL,
    p_notes TEXT
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Scout raporu ekle (onaysız olarak)
    INSERT INTO scoutreports (user_id, player_id, predicted_value, notes, report_date, is_approved)
    VALUES (p_user_id, p_player_id, p_predicted_value, p_notes, CURRENT_TIMESTAMP, FALSE);

    RAISE NOTICE 'Scout raporu oluşturuldu (onay bekliyor): Oyuncu %, Kullanıcı %, Tahmin %',
                 p_player_id, p_user_id, p_predicted_value;
END;
$$;

\echo '   ✓ sp_CreateScoutReport güncellendi'

-- 4. Onay için yeni stored procedure
\echo ''
\echo '4. sp_ApproveScoutReport stored procedure oluşturuluyor...'

CREATE OR REPLACE PROCEDURE sp_ApproveScoutReport(
    p_report_id INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE scoutreports
    SET is_approved = TRUE
    WHERE report_id = p_report_id;

    RAISE NOTICE 'Scout raporu onaylandı: Rapor ID %', p_report_id;
END;
$$;

\echo '   ✓ sp_ApproveScoutReport oluşturuldu'

-- 5. Reddetme için stored procedure
\echo ''
\echo '5. sp_RejectScoutReport stored procedure oluşturuluyor...'

CREATE OR REPLACE PROCEDURE sp_RejectScoutReport(
    p_report_id INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM scoutreports
    WHERE report_id = p_report_id;

    RAISE NOTICE 'Scout raporu reddedildi ve silindi: Rapor ID %', p_report_id;
END;
$$;

\echo '   ✓ sp_RejectScoutReport oluşturuldu'

-- 6. Kontrol sorguları
\echo ''
\echo '6. Kontrol yapılıyor...'

\echo '   Onaylı raporlar:'
SELECT COUNT(*) as approved_reports FROM scoutreports WHERE is_approved = TRUE;

\echo '   Onay bekleyen raporlar:'
SELECT COUNT(*) as pending_reports FROM scoutreports WHERE is_approved = FALSE;

\echo ''
\echo '========================================='
\echo 'KURULUM TAMAMLANDI!'
\echo '========================================='
\echo ''
\echo 'Kullanım:'
\echo '- Yeni raporlar otomatik onaysız eklenir'
\echo '- Admin onay/red işlemlerini yapabilir'
\echo '- Normal kullanıcılar sadece onaylı raporları görür'
\echo ''
