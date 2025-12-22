# ⚽ Futbol Scout Web Uygulaması

Profesyonel futbol oyuncuları için yapay zeka destekli skaut ve piyasa değeri tahmin sistemi.

## 🚀 Hızlı Başlangıç

### Otomatik Kurulum (Windows)
```bash
# 1. Veritabanı kurulumu
setup_database.bat

# 2. Python kurulumu
setup_python.bat

# 3. Node.js kurulumu
setup_nodejs.bat

# 4. .NET build
cd web_ui\ScoutWeb && dotnet build

# 5. Tüm servisleri başlat
start_all_windows.bat
```

### Web Arayüzü
```
http://localhost:5199
```

**Login:** `admin` / `123456`

---

## 📋 Özellikler

- ✅ Oyuncu yönetimi (CRUD)
- ✅ Transfermarkt web scraping
- ✅ Yapay zeka ile değer tahmini
- ✅ Scout raporu sistemi (onay mekanizması)
- ✅ Admin paneli
- ✅ 6 Katmanlı SOA mimarisi
- ✅ REST, SOAP, gRPC protokolleri
- ✅ BCrypt şifreleme
- ✅ Role-based authorization

---

## 🏗️ Mimari

### 6 Katmanlı SOA
```
1. Presentation Layer (Controllers)
2. Business Logic Layer (Services)
3. Data Access Layer (Repositories)
4. Domain Model Layer (Models)
5. Data Context Layer (EF Core)
6. Cross-Cutting Concerns (Middleware)
```

### Servisler
```
ASP.NET (5199) → Python ML (5000)
                 Node.js API (3000)
                 gRPC (5001)
                 ↓
             PostgreSQL (5432)
```

---

## 🛠️ Teknolojiler

- ASP.NET Core 8.0
- PostgreSQL 14+
- Node.js 18+
- Python 3.9+
- Entity Framework Core
- gRPC, SOAP, REST
- Bootstrap 5
- scikit-learn

---

## 📚 Dokümantasyon

- [KURULUM_REHBERI.md](KURULUM_REHBERI.md) - Detaylı kurulum
- [HIZLI_BASLATMA.md](HIZLI_BASLATMA.md) - Hızlı başlat
- [SOA_MIMARISI_DOKUMANTASYONU.md](SOA_MIMARISI_DOKUMANTASYONU.md) - Mimari

---

## 🎮 Kullanım

### Sayfalar
- `/Player` - Oyuncu listesi
- `/Player/Create` - Oyuncu ekle
- `/Reports/ScoutReport` - Scout raporları
- `/Reports/AdminDashboard` - Admin paneli

### Scout Raporu Akışı
1. Kullanıcı rapor ekler (onaysız)
2. Admin onaylar/reddeder
3. Onaylanan raporlar herkese gösterilir

---

**Detaylı bilgi için [KURULUM_REHBERI.md](KURULUM_REHBERI.md) dosyasını inceleyin.**
