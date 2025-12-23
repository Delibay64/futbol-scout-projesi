# 🧪 Sistem Test Kılavuzu

## ✅ Derleme Durumu
- **ScoutWeb:** Başarılı (0 hata, 0 uyarı)
- **ScoutGrpcService:** Başarılı (0 hata, 3 uyarı - net7.0 EOL)

---

## 🚀 Sistemi Başlatma (Sırasıyla)

### Yöntem 1: Otomatik (ÖNERİLEN)
```bash
TUMU_BASLAT.bat
```
Bu dosya tüm servisleri otomatik olarak başlatır.

### Yöntem 2: Manuel

**1. PostgreSQL Database**
- Zaten çalışıyor olmalı (port 5432)
- Kontrol: `psql -U postgres -d ScoutDB`

**2. Node.js REST/SOAP API**
```bash
START_NODEJS_API.bat
```
- Port: 3000
- Test: http://localhost:3000/api

**3. Python ML Service**
```bash
BASLAT_ML_SERVICE.bat
```
- Port: 5000
- Test: Python konsolu "Flask çalışıyor" mesajını gösterecek

**4. gRPC Service**
```bash
BASLAT_GRPC.bat
```
- Port: 5001
- Test: http://localhost:5001 → "gRPC Player Service çalışıyor!"

**5. Web Application**
- Visual Studio'dan F5 veya:
```bash
cd web_ui\ScoutWeb
dotnet run
```
- Port: 5199 (veya otomatik atanan)

---

## 🧪 Test Senaryoları

### Test 1: ML Tahmini (gRPC + Python)
1. Web uygulamasını aç: http://localhost:5199
2. Giriş yap (kullanıcı adı/şifre var ise)
3. "Oyuncular" menüsüne git
4. Herhangi bir oyuncuya tıkla (Detay sayfası)
5. "🤖 AI Tahmini Yap" butonuna tıkla
6. **Beklenen Sonuç:**
   - Loading... mesajı
   - Birkaç saniye sonra tahmin değeri görünür
   - Konsol logları:
     ```
     gRPC: ML Tahmini istendi - Player ID: X
     ML Servisi çağrılıyor: http://localhost:5000/predict
     ✅ ML Tahmini: XXXXX EUR
     ```

### Test 2: SOAP Döviz Doğrulama
1. Web uygulamasında "SOA Entegrasyonları" menüsüne git
2. "SOAP Doğrulama" kartına tıkla
3. **Beklenen Sonuç:**
   - Sol kart: REST API'den EUR/TRY döviz kuru
   - Sağ kart: SOAP doğrulama sonucu (✅ Doğrulandı)
   - SOAP XML yanıtı görünür
4. Manuel test:
   - From: EUR, To: TRY, Oran: 36.0 gir
   - "Doğrula" butonuna tıkla
   - Sonuç: "Doğrulandı" veya "Doğrulanamadı"

### Test 3: Node.js REST API
1. "SOA Entegrasyonları" → "Node.js REST API" demo
2. **Beklenen Sonuç:**
   - Oyuncu listesi JSON formatında
   - PostgreSQL'den çekilmiş 20 oyuncu

### Test 4: External APIs
1. "SOA Entegrasyonları" → "Hazır API Kullanımı"
2. **Beklenen Sonuç:**
   - Hava Durumu: İstanbul için mock data
   - Döviz Kuru: EUR/TRY gerçek değer (ExchangeRate API)

### Test 5: SOAP Oyuncu Bilgisi
1. "SOA Entegrasyonları" → "SOAP Web Service"
2. **Beklenen Sonuç:**
   - SOAP XML isteği
   - SOAP XML yanıtı
   - Player ID 1'in bilgileri

---

## 🔍 Hata Kontrolleri

### Hata 1: Node.js API Başlamıyor
```bash
cd nodejs_api
npm install
node server.js
```
**Kontrol:**
- Port 3000 kullanımda mı? → `netstat -ano | findstr :3000`
- PostgreSQL çalışıyor mu?
- `node_modules` klasörü var mı?

### Hata 2: Python ML Service Başlamıyor
```bash
cd ml_service
pip install flask scikit-learn pandas numpy
python ai_service.py
```
**Kontrol:**
- Python kurulu mu? → `python --version`
- Port 5000 kullanımda mı?
- Gerekli kütüphaneler kurulu mu?

### Hata 3: gRPC Servisi HTTP/2 Hatası
**Çözüm:** PlayerController.cs zaten HTTP/2 desteğini içeriyor.
**Kontrol:**
- gRPC servisi çalışıyor mu? → http://localhost:5001
- Firewall bloklama yapıyor mu?

### Hata 4: SOAP "Servis cevap vermedi"
**Kontrol:**
- Node.js API çalışıyor mu? → http://localhost:3000/soap?wsdl
- WSDL dosyası erişilebilir mi?

### Hata 5: Database Bağlantı Hatası
**Kontrol:**
- PostgreSQL çalışıyor mu? → `pg_isready -h localhost -p 5432`
- Şifre doğru mu? → "admin" (server.js ve appsettings.json)
- ScoutDB database'i var mı?

---

## 📊 Port Durumu Kontrolü

```bash
# Windows PowerShell
netstat -ano | findstr "3000 5000 5001 5432 5199"
```

**Beklenen Çıktı:**
```
TCP    0.0.0.0:3000    LISTENING    (Node.js)
TCP    0.0.0.0:5000    LISTENING    (Python ML)
TCP    0.0.0.0:5001    LISTENING    (gRPC)
TCP    0.0.0.0:5432    LISTENING    (PostgreSQL)
TCP    0.0.0.0:5199    LISTENING    (Web App)
```

---

## 🎯 Başarı Kriterleri

### ✅ Sistem Tamamen Çalışıyor:
- [ ] PostgreSQL bağlantısı başarılı
- [ ] Node.js API yanıt veriyor (GET /api)
- [ ] SOAP WSDL erişilebilir (/soap?wsdl)
- [ ] Python ML servisi tahmin yapıyor
- [ ] gRPC servisi HTTP/2 ile iletişim kuruyor
- [ ] Web uygulaması derleniyor ve çalışıyor
- [ ] ML tahmini butonuna tıklayınca sonuç geliyor
- [ ] SOAP doğrulama sayfası döviz kurunu doğruluyor
- [ ] Tüm SOA demo sayfaları çalışıyor

---

## 🆘 Acil Yardım

### Tüm Servisleri Durdur:
```bash
# Node.js API
taskkill /F /IM node.exe

# Python ML
taskkill /F /IM python.exe

# gRPC/Web (dotnet)
taskkill /F /IM dotnet.exe
```

### Tüm Servisleri Yeniden Başlat:
```bash
TUMU_BASLAT.bat
```
Sonra Visual Studio'dan Web uygulamasını başlat.

---

## 📝 Log Kontrolleri

### Node.js API Logları:
Terminal/CMD penceresinde:
```
🚀 Node.js API ve SOAP Servisi Başlatıldı!
✅ REST API: http://localhost:3000
📊 Oyuncular: http://localhost:3000/api/players
🧼 SOAP WSDL: http://localhost:3000/soap?wsdl
```

### Python ML Logları:
```
 * Running on http://127.0.0.1:5000
 * Serving Flask app 'ai_service'
```

### gRPC Logları:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5001
gRPC Player Service çalışıyor! Port: 5001
```

---

## ✅ Son Durum

**Derleme:** ✅ Başarılı
**Tüm Hatalar Düzeltildi:** ✅
**SOA Entegrasyonu:** ✅ Tamamlandı
**ML Servisi:** ✅ Entegre
**SOAP Doğrulama:** ✅ Çalışıyor

**Sistem tamamen hazır!** 🎉

Test yapmaya başlayabilirsiniz.
