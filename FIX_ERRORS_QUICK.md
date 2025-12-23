# 🔧 Hızlı Hata Düzeltme Kılavuzu

## ❌ Hata 1: Scout Raporları - "column s.is_approved does not exist"

### Sorun
`scoutreports` tablosunda `is_approved` kolonu eksik.

### Çözüm

PostgreSQL'de bu SQL'i çalıştırın:

```sql
-- Veritabanına bağlan
psql -U postgres -d scoutdb

-- Kolonu ekle
ALTER TABLE scoutreports
ADD COLUMN IF NOT EXISTS is_approved BOOLEAN DEFAULT FALSE;

-- Mevcut kayıtları güncelle
UPDATE scoutreports
SET is_approved = FALSE
WHERE is_approved IS NULL;

-- Kontrol et
SELECT * FROM scoutreports LIMIT 5;
```

**VEYA** hazır SQL dosyasını çalıştırın:

```bash
cd database
psql -U postgres -d scoutdb -f fix_scoutreport_column.sql
```

### Doğrulama

Uygulamayı yeniden başlattıktan sonra:
1. Scout Raporları sayfasına git
2. Hata gitmeli ✅

---

## ❌ Hata 2: Player Arama - "Bu localhost sayfası bulunamıyor"

### Sorun
`http://localhost:5199/Players?searchString=ronaldo` → HTTP ERROR 404

### Olası Nedenler ve Çözümler

#### 1. Uygulama Çalışmıyor

**Kontrol:**
```bash
cd web_ui/ScoutWeb
dotnet run
```

**Beklenen Çıktı:**
```
Now listening on: http://localhost:5199
Now listening on: https://localhost:7199
```

#### 2. Yanlış URL Kullanıyorsunuz

**Doğru URL:**
```
http://localhost:5199/Player?searchString=ronaldo
```

**YANLIŞ (çoğul):**
```
http://localhost:5199/Players?searchString=ronaldo  ❌
```

Controller adı `PlayerController` → Route: `/Player` (tekil)

#### 3. Port Farklı

`launchSettings.json`'ı kontrol edin:

```bash
cat web_ui/ScoutWeb/Properties/launchSettings.json
```

Port numarasını bulun ve ona göre URL kullanın.

#### 4. Servis Kayıt Hatası

`Program.cs`'de servisler kayıtlı mı kontrol edin:

```csharp
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
```

---

## ✅ Hızlı Test

### 1. Veritabanı Düzeltmesi

```bash
cd c:\Users\ibos_\Desktop\projeler\futbol_Scout_Projesi\database
psql -U postgres -d scoutdb
```

```sql
ALTER TABLE scoutreports ADD COLUMN IF NOT EXISTS is_approved BOOLEAN DEFAULT FALSE;
\q
```

### 2. Uygulamayı Yeniden Başlat

```bash
cd c:\Users\ibos_\Desktop\projeler\futbol_Scout_Projesi\web_ui\ScoutWeb

# Eski process'i kapat (VS Code'da Ctrl+C)
dotnet clean
dotnet build
dotnet run
```

### 3. Doğru URL'leri Test Et

**Ana Sayfa:**
```
http://localhost:5199/
```

**Oyuncu Listesi:**
```
http://localhost:5199/Player
```

**Oyuncu Arama:**
```
http://localhost:5199/Player?searchString=ronaldo
```

**Scout Raporları:**
```
http://localhost:5199/ScoutReport
```

---

## 🔍 Debugging

Eğer hâlâ 404 alıyorsanız:

1. **Terminal'deki Hata Mesajlarını Kontrol Edin**
   ```
   dotnet run
   ```
   Kırmızı hata mesajları varsa okuyun.

2. **Browser Console'u Kontrol Edin**
   - F12 tuşuna basın
   - Console sekmesini açın
   - Kırmızı hatalar varsa okuyun

3. **Routing Kontrolü**

   `Program.cs`'de routing'i kontrol edin:
   ```csharp
   app.MapControllerRoute(
       name: "default",
       pattern: "{controller=Home}/{action=Index}/{id?}");
   ```

4. **Controller Namespace'i Kontrol Edin**

   `PlayerController.cs`:
   ```csharp
   namespace ScoutWeb.Controllers  // Doğru mu?
   {
       public class PlayerController : Controller
       {
           ...
       }
   }
   ```

---

## 📝 Özet

| Hata | Çözüm | Dosya |
|------|-------|-------|
| `is_approved does not exist` | Veritabanına kolon ekle | `database/fix_scoutreport_column.sql` |
| `404 - Players not found` | URL'i `/Player` (tekil) olarak değiştir | - |
| `404 - localhost bulunamıyor` | Uygulamayı `dotnet run` ile başlat | - |
| Scout Raporları hatası | `ALTER TABLE` SQL'i çalıştır | PostgreSQL |

---

## 🎯 Adım Adım Fix

```bash
# 1. Veritabanını düzelt
psql -U postgres -d scoutdb -c "ALTER TABLE scoutreports ADD COLUMN IF NOT EXISTS is_approved BOOLEAN DEFAULT FALSE;"

# 2. Uygulamayı temizle ve rebuild et
cd web_ui/ScoutWeb
dotnet clean
dotnet build

# 3. Çalıştır
dotnet run

# 4. Browser'da test et
# http://localhost:5199/Player
```

**Başarılı olduğunda:**
- ✅ Scout Raporları sayfası açılır
- ✅ Player arama çalışır
- ✅ 404 hatası gitmez

---

**Son Güncelleme:** 2025-12-22
