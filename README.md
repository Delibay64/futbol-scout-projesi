# Futbol Scout Projesi

Modern web teknolojileri ve makine öğrenmesi ile geliştirilmiş profesyonel futbolcu takip ve değerleme sistemi.

## Proje Hakkında

Bu proje, futbolcu verilerini toplayan, analiz eden ve makine öğrenmesi ile oyuncu değerlerini tahmin eden kapsamlı bir scout sistemidir. Çok katmanlı SOA (Service-Oriented Architecture) mimarisi ile geliştirilmiştir.

## Teknolojiler

### Backend & Web
- **ASP.NET Core 6.0 MVC** - Ana web uygulaması
- **PostgreSQL** - Veritabanı yönetimi
- **Entity Framework Core** - ORM
- **Node.js Express** - REST API ve SOAP servisleri
- **gRPC** - Mikroservis iletişimi
- **Bootstrap 5** - Responsive tasarım

### Makine Öğrenmesi
- **Python Flask** - ML model servisi
- **scikit-learn** - Gradient Boosting model
- **pandas & numpy** - Veri işleme
- **Selenium & BeautifulSoup** - Web scraping

### SOA Katmanları
1. **Sunum Katmanı** - ASP.NET Core MVC Views
2. **İş Mantığı Katmanı** - Controllers & Services
3. **Veri Erişim Katmanı** - Entity Framework & Repository
4. **Servis Katmanı** - gRPC, SOAP, REST APIs
5. **Harici Servisler** - ExchangeRate API, ML Service
6. **Veritabanı Katmanı** - PostgreSQL

## Özellikler

### Kullanıcı Yönetimi
- Rol tabanlı yetkilendirme (Admin, User, Anonymous)
- Cookie-based kimlik doğrulama
- Session yönetimi

### Oyuncu Yönetimi
- CRUD işlemleri (Create, Read, Update, Delete)
- Transfermarkt web scraping ile otomatik veri çekme
- ML tabanlı oyuncu değeri tahmini
- İstatistik güncelleme ve takip

### Scout Raporları
- Scout kullanıcıları rapor oluşturabilir
- Admin onay sistemi (approve/reject)
- Tahmin edilen değer kayıtları

### Veri Analizi
- SQL Views ile gelişmiş raporlama
- Stored Procedures ile veri işleme
- Gol krallığı istatistikleri
- Döviz kuru çevirme (EUR/TRY)

### API Entegrasyonları
- **REST API** - Oyuncu verilerini JSON formatında sunar
- **SOAP API** - Döviz kuru doğrulama servisi
- **gRPC** - ML tahmin servisi ile iletişim
- **ExchangeRate API** - Gerçek zamanlı döviz kurları

## Kurulum

### Gereksinimler
- .NET 6.0 SDK
- PostgreSQL 14+
- Node.js 18+
- Python 3.10+

### Adımlar

1. **Veritabanını Hazırlayın**
```bash
psql -U postgres
CREATE DATABASE ScoutDB;
\c ScoutDB
\i database/1_schema.sql
\i database/2_data.sql
```

2. **Node.js Bağımlılıklarını Yükleyin**
```bash
cd nodejs_api
npm install
```

3. **Python Bağımlılıklarını Yükleyin**
```bash
cd ml_service
pip install -r requirements.txt
```

4. **Tüm Servisleri Başlatın**
```bash
TUMU_BASLAT.bat
```

Bu script sırasıyla şunları başlatır:
- Node.js API (Port 3000)
- Python ML Servisi (Port 5000)
- gRPC Servisi (Port 5001)
- ASP.NET Web Uygulaması (Port 5199)

## Kullanım

### Test Kullanıcıları
- **Admin:** admin / admin123
- **User:** scout1 / scout123

### Ana Sayfalar
- **Anasayfa:** http://localhost:5199
- **Oyuncu Listesi:** /Player/Index
- **Scout Raporları:** /Player/ScoutReports
- **Admin Paneli:** /Reports/AdminDashboard

### API Endpoint'leri
- **REST:** http://localhost:3000/api/players
- **SOAP WSDL:** http://localhost:3000/soap?wsdl
- **Döviz Kuru:** http://localhost:3000/api/exchange/EUR/TRY

## Proje Yapısı

```
futbol_Scout_Projesi/
├── web_ui/
│   ├── ScoutWeb/              # ASP.NET Core MVC
│   └── ScoutGrpcService/      # gRPC Servisi
├── nodejs_api/
│   ├── server.js              # REST & SOAP API
│   └── player.wsdl            # SOAP tanımı
├── ml_service/
│   ├── ai_service.py          # Flask ML servisi
│   ├── train_model_simple.py # Model eğitimi
│   └── models/                # Eğitilmiş modeller
├── database/
│   ├── 1_schema.sql           # Tablo yapısı
│   └── 2_data.sql             # Örnek veriler
└── TUMU_BASLAT.bat            # Başlatma scripti
```

## Lisans

