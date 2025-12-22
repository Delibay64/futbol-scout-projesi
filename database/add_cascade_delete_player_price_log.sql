-- Add CASCADE delete to player_price_log foreign key
-- This allows player deletion to automatically delete related price log records

-- Drop existing foreign key constraint
ALTER TABLE player_price_log
DROP CONSTRAINT IF EXISTS player_price_log_player_id_fkey;

-- Add it back with CASCADE delete
ALTER TABLE player_price_log
ADD CONSTRAINT player_price_log_player_id_fkey
FOREIGN KEY (player_id)
REFERENCES players(player_id)
ON DELETE CASCADE;

-- Verify the constraint
SELECT
    tc.constraint_name,
    tc.table_name,
    kcu.column_name,
    rc.delete_rule
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.referential_constraints AS rc
    ON tc.constraint_name = rc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY'
    AND tc.table_name = 'player_price_log';
