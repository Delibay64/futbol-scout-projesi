# Futbol Scout Projesi - Sıfırdan Kurulum Rehberi

## Gereksinimler

### 1. Yazılımlar
- ✅ **.NET 8.0 SDK** - https://dotnet.microsoft.com/download
- ✅ **PostgreSQL 14+** - https://www.postgresql.org/download/
- ✅ **Node.js 18+** - https://nodejs.org/
- ✅ **Python 3.9+** - https://www.python.org/downloads/
- ✅ **Git** - https://git-scm.com/downloads

### 2. IDE (Opsiyonel)
- Visual Studio 2022
- Visual Studio Code
- Rider

---

## Adım 1: Projeyi Klonlayın

```bash
git clone <repository-url>
cd futbol-scout-projesi
```

---

## Adım 2: PostgreSQL Veritabanı Kurulumu

### 2.1 PostgreSQL Servisini Başlatın

**Windows:**
```bash
# PostgreSQL servisi otomatik başlar
# Kontrol için:
pg_ctl status
```

**Linux/Mac:**
```bash
sudo service postgresql start
# veya
brew services start postgresql
```

### 2.2 Veritabanını Oluşturun

```bash
# PostgreSQL'e bağlanın
psql -U postgres -h localhost

# Veritabanı oluşturun
CREATE DATABASE scoutdb;

# Çıkış
\q
```

### 2.3 Veritabanı Şemasını Yükleyin

```bash
cd database

# 1. Ana şemayı yükle
psql -h localhost -U postgres -d scoutdb -f create_scoutdb.sql

# 2. Admin kullanıcısı ekle
psql -h localhost -U postgres -d scoutdb -f insert_admin.sql

# 3. Scout report onay sistemi
psql -h localhost -U postgres -d scoutdb -f add_scoutreport_approval.sql

# 4. Player price log CASCADE delete
psql -h localhost -U postgres -d scoutdb -f add_cascade_delete_player_price_log.sql
```

**Not:** PostgreSQL şifresi istediğinde (`1234` veya kendi şifrenizi girin)

### 2.4 Veritabanı Bağlantısını Yapılandırın

**Dosya:** `web_ui/ScoutWeb/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=scoutdb;Username=postgres;Password=1234"
  }
}
```

**Önemli:** `Password=1234` kısmını kendi PostgreSQL şifrenizle değiştirin!

---

## Adım 3: Python ML Servisi Kurulumu

### 3.1 Sanal Ortam Oluşturun (Opsiyonel ama Önerilen)

```bash
cd ml_service

# Windows
python -m venv venv
venv\Scripts\activate

# Linux/Mac
python3 -m venv venv
source venv/bin/activate
```

### 3.2 Gerekli Paketleri Yükleyin

```bash
pip install flask
pip install joblib
pip install pandas
pip install numpy
pip install scikit-learn
pip install requests
pip install beautifulsoup4
```

**veya tek komutta:**

```bash
pip install flask joblib pandas numpy scikit-learn requests beautifulsoup4
```

### 3.3 ML Modelini Eğitin (İlk Kurulumda)

**Önemli:** `Final_Veriler_Kalecisiz.csv` dosyası `ml_service/` klasöründe olmalı!

```bash
cd ml_service
python train_model_simple.py
```

**Çıktı:**
```
Model egitimi basladi...
Sutun sayisi: 25
Model kaydedildi: models/futbol_zeka_sistemi.pkl
```

**Kontrol:**
```bash
ls models/
# Çıktı: futbol_zeka_sistemi.pkl (yaklaşık 1.1 MB)
```

---

## Adım 4: Node.js API Servisi Kurulumu

### 4.1 Node.js Paketlerini Yükleyin

```bash
cd nodejs_api
npm install
```

**Yüklenecek Paketler:**
- express (5.2.1)
- cors (2.8.5)
- pg (8.16.3)
- soap (1.6.1)
- node-fetch (3.3.2)

### 4.2 Veritabanı Bağlantısını Kontrol Edin

**Dosya:** `nodejs_api/server.js` (Satır 10-16)

```javascript
const pool = new Pool({
  user: 'postgres',
  host: 'localhost',
  database: 'scoutdb',
  password: '1234',  // ⚠️ Kendi şifreniz
  port: 5432
});
```

---

## Adım 5: .NET Projesini Derleyin

### 5.1 Bağımlılıkları Yükleyin

```bash
cd web_ui/ScoutWeb
dotnet restore
```

### 5.2 Projeyi Derleyin

```bash
dotnet build
```

**Başarılı Çıktı:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## Adım 6: Tüm Servisleri Başlatın

### 6.1 PostgreSQL (Zaten Çalışıyor Olmalı)

```bash
# Kontrol
psql -h localhost -U postgres -c "SELECT version();"
```

### 6.2 Python ML Servisi Başlatın (Terminal 1)

```bash
cd ml_service
python simple_service.py
```

**Çıktı:**
```
Model yukleniyor: C:\...\ml_service\models\futbol_zeka_sistemi.pkl
Model basariyla yuklendi!
Flask servisi 5000 portunda calisiyor...
 * Running on http://127.0.0.1:5000
```

**Test:**
```bash
curl http://localhost:5000/
# Beklenen: 404 (normal, endpoint yok)
```

### 6.3 Node.js API Servisi Başlatın (Terminal 2)

```bash
cd nodejs_api
node server.js
```

**Çıktı:**
```
Node.js API running on http://localhost:3000
```

**Test:**
```bash
curl http://localhost:3000/api/players
# Beklenen: JSON array of players
```

### 6.4 ASP.NET Core Web UI Başlatın (Terminal 3)

```bash
cd web_ui/ScoutWeb
dotnet run
```

**Çıktı:**
```
Now listening on: https://localhost:7139
Now listening on: http://localhost:5199
Application started. Press Ctrl+C to shut down.
```

**Tarayıcıda Aç:**
```
https://localhost:7139
veya
http://localhost:5199
```

---

## Adım 7: gRPC Servisi Başlatın (Opsiyonel - Terminal 4)

```bash
cd web_ui/ScoutGrpcService
dotnet run
```

**Çıktı:**
```
Now listening on: http://localhost:5001
gRPC service running on port 5001
```

---

## Doğrulama ve Test

### 1. Web UI Erişimi
```
http://localhost:5199
```

**Login:**
- Username: `admin`
- Password: `123456`

### 2. Python ML Servisi
```bash
curl -X POST http://localhost:5000/scrape_player \
  -H "Content-Type: application/json" \
  -d "{\"name\": \"Lionel Messi\"}"
```

### 3. Node.js API
```bash
curl http://localhost:3000/api/players
```

### 4. SOAP Servisi
```bash
curl http://localhost:3000/soap?wsdl
```

### 5. gRPC Servisi
```bash
curl http://localhost:5001/api/player/1
```

---

## Servis Portları Özeti

| Servis | Port | URL |
|--------|------|-----|
| **ASP.NET Web UI** | 5199 (HTTP) / 7139 (HTTPS) | http://localhost:5199 |
| **Python ML Service** | 5000 | http://localhost:5000 |
| **Node.js API** | 3000 | http://localhost:3000 |
| **gRPC Service** | 5001 | http://localhost:5001 |
| **PostgreSQL** | 5432 | localhost:5432 |

---

## Hata Giderme

### PostgreSQL Bağlantı Hatası

**Hata:**
```
28P01: password authentication failed for user "postgres"
```

**Çözüm:**
1. PostgreSQL şifresini kontrol edin
2. `appsettings.json` ve `server.js` dosyalarındaki şifreyi güncelleyin

### ML Model Bulunamadı

**Hata:**
```
FileNotFoundError: [Errno 2] No such file or directory: 'models/futbol_zeka_sistemi.pkl'
```

**Çözüm:**
```bash
cd ml_service
python train_model_simple.py
```

### Port Zaten Kullanımda

**Hata:**
```
Address already in use: 5000
```

**Çözüm:**
```bash
# Windows
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Linux/Mac
lsof -ti:5000 | xargs kill -9
```

### Node.js Modülleri Eksik

**Hata:**
```
Error: Cannot find module 'express'
```

**Çözüm:**
```bash
cd nodejs_api
npm install
```

---

## Geliştirme İpuçları

### 1. Servisleri Tek Tek Test Edin
```bash
# PostgreSQL
psql -h localhost -U postgres -d scoutdb -c "SELECT COUNT(*) FROM players;"

# Python
curl http://localhost:5000/

# Node.js
curl http://localhost:3000/api/players

# ASP.NET
curl http://localhost:5199/
```

### 2. Logları İzleyin
- **ASP.NET:** Terminal çıktısı
- **Python:** Terminal çıktısı
- **Node.js:** Terminal çıktısı
- **PostgreSQL:** `log/postgresql-*.log`

### 3. Veritabanı Yedekleme
```bash
pg_dump -h localhost -U postgres scoutdb > backup.sql
```

### 4. Veritabanı Restore
```bash
psql -h localhost -U postgres -d scoutdb < backup.sql
```

---

## Üretim Ortamı İçin Notlar

### 1. Şifreleri Güvenli Yönetin
- Environment variables kullanın
- `appsettings.json` dosyasını `.gitignore`'a ekleyin

### 2. HTTPS Sertifikası
```bash
dotnet dev-certs https --trust
```

### 3. Güvenlik Duvarı Ayarları
```bash
# PostgreSQL (5432)
# Python ML (5000)
# Node.js API (3000)
# ASP.NET (5199/7139)
# gRPC (5001)
```

---

## Özet Başlatma Komutu (Tüm Servisler)

**Ayrı terminallerde çalıştırın:**

```bash
# Terminal 1 - Python ML
cd ml_service && python simple_service.py

# Terminal 2 - Node.js API
cd nodejs_api && node server.js

# Terminal 3 - ASP.NET Web UI
cd web_ui/ScoutWeb && dotnet run

# Terminal 4 - gRPC (Opsiyonel)
cd web_ui/ScoutGrpcService && dotnet run
```

---

## Destek

**Sorun mu yaşıyorsunuz?**

1. Tüm servislerin çalıştığından emin olun
2. Port çakışması olmadığını kontrol edin
3. Veritabanı bağlantısını test edin
4. Log dosyalarını inceleyin

**Hızlı Kontrol:**
```bash
# PostgreSQL
pg_isready -h localhost -p 5432

# Python
curl http://localhost:5000/

# Node.js
curl http://localhost:3000/api/players

# ASP.NET
curl http://localhost:5199/
```

---

## Lisans ve Geliştirici

**Proje:** Futbol Scout Web Uygulaması
**Mimari:** 6 Katmanlı SOA
**Teknolojiler:** ASP.NET Core, PostgreSQL, Python, Node.js, gRPC, SOAP
**Geliştirme:** 2025

---

**Kurulum Tamamlandı! 🎉**

Artık projeyi kullanmaya başlayabilirsiniz:
- Admin paneli: http://localhost:5199 (admin/123456)
- Oyuncu listesi: http://localhost:5199/Player
- Scout raporları: http://localhost:5199/Reports/ScoutReport
