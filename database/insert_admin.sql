-- Admin kullanıcısını ekle (BCrypt hash ile)
-- Şifre: 123456
INSERT INTO users (username, password_hash, email, role_id, created_at)
VALUES ('admin', '$2a$11$mOnXvnF4IG3vFs3.0pyQ7eHVNROThVLKXIN6L3orXVW4N.VaVpjYy', 'admin@scout.com', 1, CURRENT_TIMESTAMP);