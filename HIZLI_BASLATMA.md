# Hızlı Başlatma Rehberi

## İlk Kurulum (Tek Sefer)

### 1. Veritabanı Kurulumu
```bash
setup_database.bat
```
- PostgreSQL şifresini sorar
- `scoutdb` veritabanı oluşturur
- Şemaları yükler
- Admin kullanıcısı ekler (admin/123456)

### 2. Python Kurulumu
```bash
setup_python.bat
```
- Sanal ortam oluşturur
- Gerekli paketleri yükler
- ML modelini eğitir

### 3. Node.js Kurulumu
```bash
setup_nodejs.bat
```
- NPM paketlerini yükler
- Bağlantı ayarlarını gösterir

### 4. .NET Build
```bash
cd web_ui\ScoutWeb
dotnet build
```

---

## Günlük Kullanım

### Tüm Servisleri Başlat (TEK KOMUT)
```bash
start_all_windows.bat
```

Bu komut otomatik olarak başlatır:
- ✅ Python ML Servisi (Port 5000)
- ✅ Node.js API (Port 3000)
- ✅ ASP.NET Web UI (Port 5199/7139)

### Manuel Başlatma (Ayrı Terminaller)

**Terminal 1 - Python ML:**
```bash
cd ml_service
python simple_service.py
```

**Terminal 2 - Node.js API:**
```bash
cd nodejs_api
node server.js
```

**Terminal 3 - Web UI:**
```bash
cd web_ui\ScoutWeb
dotnet run
```

---

## Hızlı Test

### 1. Web UI
```
http://localhost:5199
```
Login: `admin` / `123456`

### 2. Python ML
```bash
curl http://localhost:5000/
```

### 3. Node.js API
```bash
curl http://localhost:3000/api/players
```

---

## Sorun Giderme

### Port Çakışması
```bash
# Port 5000 kullanımda
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Port 3000 kullanımda
netstat -ano | findstr :3000
taskkill /PID <PID> /F
```

### PostgreSQL Çalışmıyor
```bash
# Servis durumunu kontrol et
pg_isready -h localhost -p 5432

# Servisi başlat (Windows)
net start postgresql-x64-14
```

### ML Model Yok
```bash
cd ml_service
python train_model_simple.py
```

---

## Detaylı Dokümantasyon

Daha fazla bilgi için:
- [KURULUM_REHBERI.md](KURULUM_REHBERI.md) - Detaylı kurulum
- [SOA_MIMARISI_DOKUMANTASYONU.md](SOA_MIMARISI_DOKUMANTASYONU.md) - Mimari detayları
