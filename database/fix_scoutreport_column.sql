-- Scout Reports tablosuna is_approved kolonu ekleme
-- Bu SQL'i PostgreSQL'de çalıştırın

-- Kolon ekle
ALTER TABLE scoutreports
ADD COLUMN IF NOT EXISTS is_approved BOOLEAN DEFAULT FALSE;

-- Mevcut kayıtları güncelle
UPDATE scoutreports
SET is_approved = FALSE
WHERE is_approved IS NULL;

-- Başarı mesajı
SELECT 'is_approved kolonu başarıyla eklendi!' AS sonuc;
