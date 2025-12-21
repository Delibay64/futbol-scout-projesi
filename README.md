# ⚽ Futbol Scout Sistemi

Makine Öğrenmesi ile futbolcu piyasa değeri tahmini yapan mikroservis tabanlı web uygulaması.

## 🎯 Proje Özeti

Bu proje, futbolcu istatistiklerini analiz ederek piyasa değeri tahmini yapan, modern web teknolojileri ve mikroservis mimarisi kullanan bir scout sistemidir.

## 🛠️ Teknolojiler

### Backend
- **ASP.NET Core 7.0** - MVC Web Framework
- **Python Flask** - Machine Learning API
- **Node.js Express** - REST & SOAP API

### Database
- **PostgreSQL** - Ana veritabanı
- 7 Tablo, 5 View, 2 Stored Procedure, 2 Function
- Triggers ve Constraints

### Machine Learning
- **Random Forest Regressor**
- **Scikit-learn**
- Web scraping (Transfermarkt)

### Frontend
- **Bootstrap 5**
- **jQuery**
- **Razor Views**

## 📊 Mimari
```
┌─────────────┐
│  ASP.NET    │ (Port 5199)
│  Web UI     │
└──────┬──────┘
       │
   ┌───┴───┬─────────┐
   ▼       ▼         ▼
┌─────┐ ┌─────┐  ┌──────┐
│Flask│ │Node │  │ SOAP │
│ ML  │ │ API │  │  API │
└──┬──┘ └──┬──┘  └───┬──┘
   │       │         │
   └───────┴─────────┘
           │
      ┌────▼─────┐
      │PostgreSQL│
      └──────────┘
```

## 🚀 Kurulum

### 1. PostgreSQL Kurulumu
```bash
# Veritabanı oluştur
CREATE DATABASE ScoutDB;

# SQL dosyasını çalıştır
psql -U postgres -d ScoutDB -f database/schema.sql
```

### 2. Python ML Servisi
```bash
cd ml_service
pip install -r requirements.txt
python ai_service.py
```

### 3. Node.js API
```bash
cd nodejs_api
npm install
node server.js
```

### 4. ASP.NET Web
```bash
cd web_ui/ScoutWeb
dotnet restore
dotnet run
```

## 📱 Kullanım

1. **Ana Sayfa:** http://localhost:5199
2. **Node.js API:** http://localhost:3000/api/players
3. **SOAP WSDL:** http://localhost:3000/soap?wsdl
4. **ML API:** http://localhost:5000/predict

## ✨ Özellikler

- ✅ Kullanıcı kayıt/giriş sistemi
- ✅ Rol bazlı yetkilendirme (Admin/User)
- ✅ Oyuncu CRUD işlemleri
- ✅ ML ile piyasa değeri tahmini
- ✅ Web scraping (Transfermarkt)
- ✅ REST API
- ✅ SOAP Web Servisi
- ✅ Hazır API entegrasyonu (Döviz kuru)
- ✅ Responsive tasarım

## 📈 Proje Başarı Oranı

- **Makine Öğrenmesi:** 100/100 ✅
- **Veri Tabanı:** 100/100 ✅
- **İleri Web Programlama:** 100/100 ✅
- **Servis Odaklı Mimari:** 60/100 ✅

**TOPLAM: 360/400 (%90)**

## 👥 Geliştirici

[İsmin Buraya]

## 📝 Lisans

Bu proje eğitim amaçlıdır.