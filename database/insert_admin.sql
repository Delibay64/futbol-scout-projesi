-- Admin kullanıcısını ekle (BCrypt hash ile)
INSERT INTO users (username, password_hash, email, role_id, created_at)
VALUES ('admin', '$2a$11$gRRfoWczSqRfZED32y2FQeOJcu5zPhOZjZQznpRzsBu4MY6Ta.5.y', 'admin@scout.com', 1, CURRENT_TIMESTAMP);