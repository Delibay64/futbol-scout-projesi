# 🎯 STORED PROCEDURE KULLANIM KILAVUZU

Bu dokümanda yeni eklenen 3 Stored Procedure'ün kullanımı açıklanmaktadır.

---

## 📋 STORED PROCEDURE'LER

### 1. **sp_UpdateValue** (Mevcut - Oyuncu Değerini Güncelle)

**Amaç:** Oyuncunun piyasa değerini belirtilen yüzde oranında artırır veya azaltır.

**Parametre Listesi:**
- `p_player_id` (INT): Oyuncu ID
- `p_percentage` (INT): Artış/azalış yüzdesi (örn: 10 = %10 artış, -5 = %5 azalış)

**SQL Kullanımı:**
```sql
CALL sp_UpdateValue(1, 10);  -- 1 numaralı oyuncunun değerini %10 artır
```

**C# Kullanımı:**
```csharp
// ReportsController.cs (Satır 74-78)
await _context.Database.ExecuteSqlRawAsync(
    "CALL sp_UpdateValue({0}, {1})",
    playerId,
    percentage
);
```

**Arayüz Kullanımı:**
- **Sayfa:** Reports/AdminDashboard
- **Form:** "Yönetici İşlemi: Zam Yap" kartı
- **Girdi:** Oyuncu ID, Zam Oranı (%)

---

### 2. **sp_CreateScoutReport** (YENİ - Scout Raporu Oluştur)

**Amaç:** Belirtilen oyuncu için scout raporu oluşturur.

**Parametre Listesi:**
- `p_user_id` (INT): Raporu oluşturan kullanıcı ID
- `p_player_id` (INT): Oyuncu ID
- `p_predicted_value` (DECIMAL): Tahmini değer (€)
- `p_notes` (TEXT): Scout notları

**SQL Kullanımı:**
```sql
CALL sp_CreateScoutReport(1, 5, 15000000.00, 'Çok yetenekli genç forvet. Transfer önerilir.');
```

**C# Kullanımı:**
```csharp
// PlayerController.cs (Satır 449-452)
await _context.Database.ExecuteSqlRawAsync(
    "CALL sp_CreateScoutReport({0}, {1}, {2}, {3})",
    user.UserId, playerId, predictedValue, notes ?? ""
);
```

**Arayüz Kullanımı:**
- **Sayfa:** Player/Details
- **Buton:** "Scout Raporu Oluştur" (yeşil buton)
- **Modal:** Scout Raporu Formu
- **Girdi:** Tahmini Değer (€), Notlar

**Kullanım Senaryosu:**
1. Oyuncu detay sayfasına git (Player/Details/5)
2. "Scout Raporu Oluştur" butonuna tıkla
3. Tahmini değer ve notları gir
4. "Rapor Oluştur" butonuna bas
5. Rapor `scoutreports` tablosuna kaydedilir

---

### 3. **sp_UpdatePlayerStats** (YENİ - Oyuncu İstatistiklerini Güncelle)

**Amaç:** Oyuncunun sezonluk istatistiklerini günceller veya yeni sezon ekler.

**Parametre Listesi:**
- `p_player_id` (INT): Oyuncu ID
- `p_season` (VARCHAR): Sezon (örn: '2024-25')
- `p_matches` (INT): Maç sayısı
- `p_goals` (INT): Gol sayısı
- `p_assists` (INT): Asist sayısı
- `p_yellow_cards` (INT, optional): Sarı kart (varsayılan: 0)
- `p_red_cards` (INT, optional): Kırmızı kart (varsayılan: 0)
- `p_minutes` (INT, optional): Dakika (varsayılan: 0)

**SQL Kullanımı:**
```sql
-- Yeni istatistik ekle
CALL sp_UpdatePlayerStats(1, '2024-25', 30, 20, 8, 3, 0, 2400);

-- Mevcut istatistiği güncelle (aynı oyuncu + aynı sezon varsa)
CALL sp_UpdatePlayerStats(1, '2024-25', 32, 22, 10, 4, 0, 2600);
```

**C# Kullanımı:**
```csharp
// PlayerController.cs (Satır 411-414)
await _context.Database.ExecuteSqlRawAsync(
    "CALL sp_UpdatePlayerStats({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
    playerId, season, matches, goals, assists, yellowCards, redCards, minutes
);
```

**Arayüz Kullanımı:**
- **Sayfa:** Player/Details
- **Buton:** "İstatistik Güncelle" (mavi buton)
- **Modal:** İstatistik Güncelleme Formu
- **Girdi:** Sezon, Maç Sayısı, Gol, Asist, Dakika, Sarı Kart, Kırmızı Kart

**Özellikler:**
- ✅ Aynı oyuncu + aynı sezon varsa → **UPDATE**
- ✅ Yeni sezon ise → **INSERT**
- ✅ Tekrar eden kayıt oluşturmaz

**Kullanım Senaryosu:**
1. Oyuncu detay sayfasına git (Player/Details/5)
2. "İstatistik Güncelle" butonuna tıkla
3. Sezon ve istatistikleri gir (örn: 2024-25, 30 maç, 15 gol, 5 asist)
4. "Kaydet" butonuna bas
5. İstatistik `playerstats` tablosuna eklenir/güncellenir

---

## 🎨 ARAYÜZ YENİLİKLERİ

### Player Details Sayfası (Player/Details/{id})

**Yeni Butonlar:**
```
┌─────────────────────────────────────────────────┐
│  [📊 İstatistik Güncelle]  [📝 Scout Raporu]   │
└─────────────────────────────────────────────────┘
```

**İstatistik Güncelleme Modal:**
- Sezon seçimi
- Maç, Gol, Asist girişi
- Kart ve dakika bilgisi
- Form validation

**Scout Raporu Modal:**
- Tahmini değer girişi (€)
- Not alanı (textarea)
- AJAX ile kayıt
- Başarı/hata mesajı

---

## 📊 VERİTABANI ETKİLERİ

### sp_UpdateValue
- **Güncellenen Tablo:** `players` (current_market_value)
- **Log Tablosu:** `player_price_log` (değişim geçmişi)

### sp_CreateScoutReport
- **Eklenen Tablo:** `scoutreports`
- **İlişkiler:** users (user_id), players (player_id)

### sp_UpdatePlayerStats
- **Güncellenen/Eklenen Tablo:** `playerstats`
- **Mantık:** UPSERT (INSERT or UPDATE)

---

## 🧪 TEST SENARYOLARI

### Test 1: Oyuncu Değeri Güncelleme
```sql
-- Başlangıç değeri kontrol et
SELECT player_id, full_name, current_market_value FROM players WHERE player_id = 1;

-- %20 artır
CALL sp_UpdateValue(1, 20);

-- Sonuç kontrol et
SELECT player_id, full_name, current_market_value FROM players WHERE player_id = 1;

-- Log kontrolü
SELECT * FROM player_price_log WHERE player_id = 1 ORDER BY changed_at DESC LIMIT 1;
```

### Test 2: Scout Raporu Ekleme
```sql
-- Rapor oluştur
CALL sp_CreateScoutReport(1, 5, 12000000, 'Genç ve yetenekli');

-- Sonuç kontrol et
SELECT sr.report_id, p.full_name, u.username, sr.predicted_value, sr.notes
FROM scoutreports sr
JOIN players p ON sr.player_id = p.player_id
JOIN users u ON sr.user_id = u.user_id
WHERE sr.player_id = 5
ORDER BY sr.report_date DESC;
```

### Test 3: İstatistik Güncelleme
```sql
-- İlk kayıt (INSERT)
CALL sp_UpdatePlayerStats(1, '2024-25', 10, 5, 3, 1, 0, 900);

-- Kontrol et
SELECT * FROM playerstats WHERE player_id = 1 AND season = '2024-25';

-- Güncelleme (UPDATE - aynı sezon)
CALL sp_UpdatePlayerStats(1, '2024-25', 15, 8, 5, 2, 0, 1350);

-- Kontrol et (kayıt sayısı artmamalı, değerler güncellenm eli)
SELECT * FROM playerstats WHERE player_id = 1 AND season = '2024-25';
```

---

## 🔧 SORUN GİDERME

### Hata: "procedure sp_CreateScoutReport does not exist"
**Çözüm:** Migration scriptini çalıştırın
```bash
cd database
apply_updates.bat
```

### Hata: "duplicate key value violates unique constraint"
**Neden:** sp_UpdatePlayerStats aynı oyuncu + sezon için iki kez INSERT yapıyor
**Çözüm:** Prosedürün mantığı UPSERT, bu hata oluşmamalı. Eğer oluşuyorsa prosedürü kontrol edin.

### Hata: "column 'user_id' of relation 'scoutreports' does not exist"
**Neden:** Veritabanı şeması güncel değil
**Çözüm:** `create_scoutdb.sql` dosyasını tekrar çalıştırın

---

## 📈 PERFORMANS KAZANIMI

| Önceki Yöntem | Yeni Yöntem (SP) | Kazanım |
|---------------|------------------|---------|
| 3 SQL sorgusu (SELECT, UPDATE, INSERT) | 1 CALL | 66% daha az network trafiği |
| Client-side hesaplama | Server-side hesaplama | Daha hızlı |
| Transaction yönetimi manuel | Otomatik rollback | Daha güvenli |

---

## ✅ SON KONTROL LİSTESİ

- [x] 3 Stored Procedure oluşturuldu
- [x] PlayerController'a action'lar eklendi
- [x] Player Details view'una modal'lar eklendi
- [x] AJAX entegrasyonu yapıldı
- [x] Form validation eklendi
- [x] Hata yönetimi eklendi
- [x] Test senaryoları hazırlandı

---

**Oluşturulma Tarihi:** 22 Aralık 2024
**Versiyon:** 1.0
**Proje:** Futbol Scout Projesi
