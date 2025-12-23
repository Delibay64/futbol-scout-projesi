# ⚡ Hızlı Başlangıç Kılavuzu

## 🚀 3 Adımda Sistemi Başlat

### 1️⃣ Tüm Servisleri Başlat
```bash
TUMU_BASLAT.bat
```
Bu komut şu servisleri başlatır:
- ✅ Node.js REST/SOAP API (port 3000)
- ✅ Python ML Service (port 5000)
- ✅ gRPC Service (port 5001)

### 2️⃣ Web Uygulamasını Başlat
Visual Studio'da:
- `ScoutWeb` projesini aç
- **F5** tuşuna bas

Veya terminal'de:
```bash
cd web_ui\ScoutWeb
dotnet run
```

### 3️⃣ Test Et
Tarayıcıda: **http://localhost:5199**

---

## 🎯 İlk Test - ML Tahmini

1. Giriş yap
2. "Oyuncular" → Herhangi bir oyuncu seç
3. **"🤖 AI Tahmini Yap"** butonuna tıkla
4. ✅ Tahmin değeri görünecek!

---

## 🧼 İkinci Test - SOAP Doğrulama

1. "SOA Entegrasyonları" menüsüne git
2. **"SOAP Doğrulama"** kartına tıkla
3. ✅ EUR/TRY doğrulama sonucu görünecek!

---

## ⚠️ Sorun mu var?

### Servisler başlamıyor:
```bash
cd nodejs_api
npm install
node server.js
```

### Web derleme hatası:
```bash
cd web_ui\ScoutWeb
dotnet build
```

### Python hatası:
```bash
cd ml_service
pip install flask scikit-learn pandas
python ai_service.py
```

---

## 📚 Detaylı Dokümantasyon

- **Tüm Özellikler:** [SOA_TAMAMLANDI.md](SOA_TAMAMLANDI.md)
- **Test Senaryoları:** [TEST_SISTEMI.md](TEST_SISTEMI.md)
- **Hata Düzeltmeleri:** [FIX_ERRORS_QUICK.md](FIX_ERRORS_QUICK.md)

---

## ✅ Sistem Durumu

| Servis | Port | Durum |
|--------|------|-------|
| PostgreSQL | 5432 | ✅ Çalışıyor |
| Node.js API | 3000 | ✅ Çalışıyor |
| Python ML | 5000 | ✅ Çalışıyor |
| gRPC Service | 5001 | ✅ Çalışıyor |
| Web App | 5199 | ✅ Derlendi |

**Tüm hatalar düzeltildi! Sistem tamamen hazır!** 🎉
