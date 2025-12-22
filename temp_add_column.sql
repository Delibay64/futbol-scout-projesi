-- is_approved kolonu ekle
ALTER TABLE scoutreports ADD COLUMN IF NOT EXISTS is_approved BOOLEAN DEFAULT FALSE;

-- Mevcut raporları onayla
UPDATE scoutreports SET is_approved = TRUE WHERE is_approved IS NULL OR is_approved = FALSE;

-- Kontrol
SELECT column_name, data_type, column_default
FROM information_schema.columns
WHERE table_name = 'scoutreports' AND column_name = 'is_approved';
