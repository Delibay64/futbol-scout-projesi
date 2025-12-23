-- ========================================
-- FUTBOL SCOUT PROJESİ - VERİTABANI KURULUMU
-- ========================================

-- 1. Veritabanını oluştur (eğer yoksa)
-- NOT: Bunu psql'de çalıştırın veya pgAdmin'de CREATE DATABASE ScoutDB; olarak çalıştırın

-- 2. TABLOLAR

CREATE TABLE IF NOT EXISTS teams (
    team_id SERIAL PRIMARY KEY,
    team_name VARCHAR(100),
    league_name VARCHAR(100),
    country VARCHAR(50)
);

CREATE TABLE IF NOT EXISTS players (
    player_id SERIAL PRIMARY KEY,
    full_name VARCHAR(100),
    age INT,
    position VARCHAR(50),
    nationality VARCHAR(50),
    team_id INT REFERENCES teams(team_id),
    current_market_value DECIMAL(15, 2)
);

CREATE TABLE IF NOT EXISTS playerstats (
    stat_id SERIAL PRIMARY KEY,
    player_id INT REFERENCES players(player_id),
    season VARCHAR(20),
    matches_played INT,
    goals INT,
    assists INT,
    yellow_cards INT,
    red_cards INT,
    minutes_played INT
);

CREATE TABLE IF NOT EXISTS roles (
    role_id SERIAL PRIMARY KEY,
    role_name VARCHAR(50) UNIQUE
);

CREATE TABLE IF NOT EXISTS users (
    user_id SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE,
    password_hash VARCHAR(255),
    email VARCHAR(100),
    role_id INT REFERENCES roles(role_id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS scoutreports (
    report_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id),
    player_id INT REFERENCES players(player_id),
    predicted_value DECIMAL(15, 2),
    notes TEXT,
    report_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_approved BOOLEAN DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS player_price_logs (
    log_id SERIAL PRIMARY KEY,
    player_id INT REFERENCES players(player_id) ON DELETE CASCADE,
    old_value DECIMAL(15, 2),
    new_value DECIMAL(15, 2),
    change_percentage INT,
    changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 3. FONKSİYONLAR

-- Euro'dan TL'ye çevirme fonksiyonu (örnek kur: 35)
CREATE OR REPLACE FUNCTION fn_EuroToTL(euro_value DECIMAL)
RETURNS DECIMAL AS $$
BEGIN
    RETURN euro_value * 35.0;
END;
$$ LANGUAGE plpgsql;

-- Maç başına gol ortalaması hesaplama
CREATE OR REPLACE FUNCTION fn_GoalsPerMatch(goals INT, matches INT)
RETURNS DECIMAL AS $$
BEGIN
    IF matches = 0 THEN
        RETURN 0;
    END IF;
    RETURN CAST(goals AS DECIMAL) / CAST(matches AS DECIMAL);
END;
$$ LANGUAGE plpgsql;

-- 4. VIEW'LAR

-- Oyuncu detayları (TL cinsinden)
CREATE OR REPLACE VIEW vw_playerdetailstr AS
SELECT
    p.full_name,
    p.age,
    p.position,
    t.team_name,
    p.current_market_value AS eurovalue,
    fn_EuroToTL(p.current_market_value) AS tlvalue
FROM players p
LEFT JOIN teams t ON p.team_id = t.team_id;

-- Gol krallığı
CREATE OR REPLACE VIEW vw_topscorers AS
SELECT
    p.full_name,
    SUM(ps.goals)::integer AS goals,
    SUM(ps.assists)::integer AS assists,
    fn_GoalsPerMatch(SUM(ps.goals)::integer, SUM(ps.matches_played)::integer) AS goalspermatch
FROM players p
INNER JOIN playerstats ps ON p.player_id = ps.player_id
GROUP BY p.player_id, p.full_name
HAVING SUM(ps.goals) > 0;

-- Genç yetenekler (21 yaş altı)
CREATE OR REPLACE VIEW vw_youngtalents AS
SELECT
    player_id,
    full_name,
    position,
    age,
    nationality,
    team_id,
    current_market_value
FROM players
WHERE age < 21;

-- Takım özeti
CREATE OR REPLACE VIEW vw_teamsummary AS
SELECT
    t.team_name,
    COUNT(p.player_id) AS playercount,
    AVG(p.age) AS averageage
FROM teams t
LEFT JOIN players p ON t.team_id = p.team_id
GROUP BY t.team_id, t.team_name;

-- Scout rapor özeti
CREATE OR REPLACE VIEW vw_scoutsummary AS
SELECT
    p.full_name,
    sr.report_date,
    5 AS rating, -- Örnek sabit değer
    u.username AS scoutname
FROM scoutreports sr
INNER JOIN players p ON sr.player_id = p.player_id
INNER JOIN users u ON sr.user_id = u.user_id;

-- 5. STORED PROCEDURE

-- Oyuncu değerine zam yapma
CREATE OR REPLACE PROCEDURE sp_UpdateValue(
    p_player_id INT,
    p_percentage INT
)
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
    v_new_value := v_old_value * (1 + p_percentage / 100.0);

    -- Güncelle
    UPDATE players
    SET current_market_value = v_new_value
    WHERE player_id = p_player_id;

    -- Log'a kaydet
    INSERT INTO player_price_log (player_id, old_value, new_value, change_percentage)
    VALUES (p_player_id, v_old_value, v_new_value, p_percentage);

    RAISE NOTICE 'Oyuncu % değeri güncellendi: % -> %', p_player_id, v_old_value, v_new_value;
END;
$$;

-- Stored Procedure 2: Scout Raporu Oluştur
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
    INSERT INTO scoutreports (user_id, player_id, predicted_value, notes, report_date)
    VALUES (p_user_id, p_player_id, p_predicted_value, p_notes, CURRENT_TIMESTAMP);

    RAISE NOTICE 'Scout raporu oluşturuldu: Oyuncu %, Kullanıcı %, Tahmin %', p_player_id, p_user_id, p_predicted_value;
END;
$$;

-- Stored Procedure 3: Oyuncu İstatistiği Güncelle
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
    -- Aynı oyuncu ve sezon için kayıt var mı kontrol et
    SELECT stat_id INTO v_stat_id
    FROM playerstats
    WHERE player_id = p_player_id AND season = p_season;

    IF v_stat_id IS NOT NULL THEN
        -- Güncelle
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
        -- Yeni kayıt ekle
        INSERT INTO playerstats (player_id, season, matches_played, goals, assists, yellow_cards, red_cards, minutes_played)
        VALUES (p_player_id, p_season, p_matches, p_goals, p_assists, p_yellow_cards, p_red_cards, p_minutes);

        RAISE NOTICE 'Oyuncu % için % sezonu istatistikleri eklendi', p_player_id, p_season;
    END IF;
END;
$$;

-- 6. INDEX'LER (Performans Optimizasyonu)

-- Foreign Key Index'leri (JOIN performansı için kritik)
CREATE INDEX IF NOT EXISTS idx_players_team_id ON players(team_id);
CREATE INDEX IF NOT EXISTS idx_playerstats_player_id ON playerstats(player_id);
CREATE INDEX IF NOT EXISTS idx_scoutreports_player_id ON scoutreports(player_id);
CREATE INDEX IF NOT EXISTS idx_scoutreports_user_id ON scoutreports(user_id);
CREATE INDEX IF NOT EXISTS idx_users_role_id ON users(role_id);
CREATE INDEX IF NOT EXISTS idx_player_price_log_player_id ON player_price_log(player_id);

-- Sık kullanılan filtreleme kolonları için index'ler
CREATE INDEX IF NOT EXISTS idx_players_age ON players(age);
CREATE INDEX IF NOT EXISTS idx_players_position ON players(position);
CREATE INDEX IF NOT EXISTS idx_playerstats_season ON playerstats(season);

-- Composite Index'ler (birden fazla kolon birlikte kullanıldığında)
CREATE INDEX IF NOT EXISTS idx_playerstats_player_season ON playerstats(player_id, season);
CREATE INDEX IF NOT EXISTS idx_scoutreports_date_desc ON scoutreports(report_date DESC);

-- 7. CHECK CONSTRAINT'LERİ (Veri Doğrulama)

-- Oyuncu yaşı kontrolü (0-100 arası olmalı)
ALTER TABLE players ADD CONSTRAINT chk_players_age
    CHECK (age > 0 AND age < 100);

-- Oyuncu değeri pozitif olmalı
ALTER TABLE players ADD CONSTRAINT chk_players_value
    CHECK (current_market_value >= 0);

-- İstatistikler negatif olamaz
ALTER TABLE playerstats ADD CONSTRAINT chk_playerstats_matches
    CHECK (matches_played >= 0);

ALTER TABLE playerstats ADD CONSTRAINT chk_playerstats_goals
    CHECK (goals >= 0);

ALTER TABLE playerstats ADD CONSTRAINT chk_playerstats_assists
    CHECK (assists >= 0);

ALTER TABLE playerstats ADD CONSTRAINT chk_playerstats_cards
    CHECK (yellow_cards >= 0 AND red_cards >= 0);

ALTER TABLE playerstats ADD CONSTRAINT chk_playerstats_minutes
    CHECK (minutes_played >= 0);

-- Scout raporu tahmin değeri pozitif olmalı
ALTER TABLE scoutreports ADD CONSTRAINT chk_scoutreports_value
    CHECK (predicted_value >= 0);

-- 8. ÖRNEK VERİLER

-- Roller
INSERT INTO roles (role_name) VALUES ('Admin'), ('Scout'), ('Viewer')
ON CONFLICT (role_name) DO NOTHING;

-- Admin kullanıcısı (Kullanıcı adı: admin, Şifre: admin)
-- BCrypt hash: $2a$11$gRRfoWczSqRfZED32y2FQeOJcu5zPhOZjZQznpRzsBu4MY6Ta.5.y
INSERT INTO users (username, password_hash, email, role_id)
VALUES ('admin', '$2a$11$gRRfoWczSqRfZED32y2FQeOJcu5zPhOZjZQznpRzsBu4MY6Ta.5.y', 'admin@scout.com', 1)
ON CONFLICT (username) DO NOTHING;

-- Örnek takımlar
INSERT INTO teams (team_name, league_name, country) VALUES
('Galatasaray', 'Süper Lig', 'Türkiye'),
('Fenerbahçe', 'Süper Lig', 'Türkiye'),
('Beşiktaş', 'Süper Lig', 'Türkiye'),
('Barcelona', 'La Liga', 'İspanya'),
('Real Madrid', 'La Liga', 'İspanya');

-- Örnek oyuncular
INSERT INTO players (full_name, age, position, nationality, team_id, current_market_value) VALUES
('Mauro Icardi', 31, 'Forvet', 'Arjantin', 1, 12000000),
('Dries Mertens', 36, 'Forvet', 'Belçika', 1, 2000000),
('Edin Dzeko', 38, 'Forvet', 'Bosna', 2, 1500000),
('Dusan Tadic', 35, 'Orta Saha', 'Sırbistan', 2, 3000000),
('Vincent Aboubakar', 32, 'Forvet', 'Kamerun', 3, 2500000);

-- Örnek istatistikler
INSERT INTO playerstats (player_id, season, matches_played, goals, assists, yellow_cards, red_cards, minutes_played) VALUES
(1, '2023-24', 25, 18, 5, 3, 0, 2100),
(2, '2023-24', 20, 8, 12, 1, 0, 1500),
(3, '2023-24', 30, 15, 3, 4, 1, 2400),
(4, '2023-24', 28, 6, 14, 5, 0, 2300),
(5, '2023-24', 22, 12, 2, 6, 0, 1800);

\echo 'ScoutDB veritabanı başarıyla oluşturuldu!';
