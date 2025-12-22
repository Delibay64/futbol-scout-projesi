-- Admin şifresini güncelle (BCrypt hash ile)
-- Yeni şifre: 123456
-- Hash: $2a$11$mOnXvnF4IG3vFs3.0pyQ7eHVNROThVLKXIN6L3orXVW4N.VaVpjYy

UPDATE users
SET password_hash = '$2a$11$mOnXvnF4IG3vFs3.0pyQ7eHVNROThVLKXIN6L3orXVW4N.VaVpjYy'
WHERE username = 'admin';

-- Kontrol için
SELECT user_id, username, email, LEFT(password_hash, 30) as hash_preview
FROM users
WHERE username = 'admin';
