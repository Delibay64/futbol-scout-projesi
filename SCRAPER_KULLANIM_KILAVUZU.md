# Scraper Kullanım Kılavuzu

## Sistem Mimarisi

Futbol Scout projesi **mevcut çalışan bir scraper sistemi** kullanıyor:

```
C# Web App (ASP.NET Core)
    ↓ HTTP POST
Flask API Service (Python - Port 5000)
    ↓ Web Scraping
Transfermarkt.com.tr
    ↓ HTML Parse
Oyuncu Verileri (JSON)
```

## Dosyalar

### 1. Python Flask Servisi
**Dosya:** `ml_service/ai_service.py`
**Port:** 5000
**Endpoints:**
- `/predict` - ML tahmin servisi
- `/scrape_player` - Transfermarkt scraper

### 2. C# Controller
**Dosya:** `web_ui/ScoutWeb/Controllers/PlayerController.cs`
**Method:** `FetchPlayerData(string name)` (satır 255)
- Flask API'yi çağırır
- JSON response döner

### 3. View
**Dosya:** `web_ui/ScoutWeb/Views/Player/Create.cshtml`
**Button:** "🌐 Verileri Çek"
- JavaScript ile `/Player/FetchPlayerData` çağırır
- Form alanlarını otomatik doldurur

## Nasıl Çalıştırılır?

### 1. Python Flask Servisini Başlat

```bash
cd ml_service
python ai_service.py
```

Çıktı:
```
✅ AI Servisi: Model başarıyla yüklendi!
🚀 ML Servisi 5000 portunda çalışıyor...
```

### 2. ASP.NET Core Uygulamasını Başlat

```bash
cd web_ui/ScoutWeb
dotnet run
```

### 3. Kullanım

1. Tarayıcıda `http://localhost:5199/Player/Create` aç
2. **Ad Soyad** alanına oyuncu ismini yaz (örn: "Erling Haaland")
3. **"🌐 Verileri Çek"** butonuna tıkla
4. Veriler Transfermarkt'tan çekilip otomatik doldurulur:
   - Yaş
   - Milliyet
   - Piyasa Değeri
   - Takım
   - Pozisyon
   - Gol/Asist/Maç sayısı

## Çekilen Veriler

### Transfermarkt'tan Alınan Bilgiler:

| Alan | Açıklama | Örnek |
|------|----------|-------|
| `FullName` | Oyuncu adı | "Cristiano Ronaldo" |
| `TeamName` | Takım | "Al Nassr" |
| `Position` | Mevki | "Forvet" |
| `Age` | Yaş | 39 |
| `Nationality` | Milliyet | "Portekiz" |
| `CurrentMarketValue` | Piyasa değeri (€) | 15000000 |
| `Goals` | Toplam gol | 54 |
| `Assists` | Toplam asist | 15 |
| `MatchesPlayed` | Maç sayısı | 72 |
| `MinutesPlayed` | Dakika | 6480 |

## Sorun Giderme

### Hata: "Python servisi açık mı?"

**Sebep:** Flask servisi çalışmıyor.

**Çözüm:**
```bash
cd ml_service
python ai_service.py
```

### Hata: "ModuleNotFoundError: No module named 'flask'"

**Çözüm:**
```bash
pip install flask beautifulsoup4 requests joblib pandas numpy scikit-learn
```

### Hata: "Oyuncu bulunamadı!"

**Sebepler:**
1. Oyuncu ismi yanlış yazılmış
2. Transfermarkt'ta bu isimle kayıt yok
3. Transfermarkt site yapısı değişmiş

**Çözüm:**
- Oyuncu ismini tam olarak yaz
- İngilizce karakter kullan (örn: "Mesut Ozil" yerine "Mesut Özil")

### Port 5000 kullanımda hatası

**Çözüm:**
```bash
# Windows
netstat -ano | findstr :5000
taskkill /PID <process_id> /F

# Linux/Mac
lsof -i :5000
kill -9 <process_id>
```

## Teknik Detaylar

### Flask API Endpoint

```python
@app.route('/scrape_player', methods=['POST'])
def scrape_player():
    data = request.json
    player_name = data.get('name')
    # Transfermarkt'tan veri çek...
    return jsonify(scraped_data)
```

### C# HTTP Client Çağrısı

```csharp
[HttpPost]
public async Task<IActionResult> FetchPlayerData(string name)
{
    var payload = new { name = name };
    var jsonContent = new StringContent(
        JsonSerializer.Serialize(payload),
        Encoding.UTF8,
        "application/json"
    );

    var response = await client.PostAsync(
        "http://localhost:5000/scrape_player",
        jsonContent
    );

    return Content(await response.Content.ReadAsStringAsync());
}
```

### JavaScript Fetch

```javascript
fetch('/Player/FetchPlayerData?name=' + encodeURIComponent(name), {
    method: 'POST'
})
.then(response => response.json())
.then(data => {
    document.getElementById("txtAge").value = data.Age;
    document.getElementById("txtNationality").value = data.Nationality;
    // ...
});
```

## Sınırlamalar

⚠️ **Rate Limiting:** Transfermarkt çok fazla istek gönderirseniz IP'nizi geçici olarak engelleyebilir.

⚠️ **Veri Doğruluğu:** Transfermarkt'ın HTML yapısı değişirse scraper çalışmayabilir.

⚠️ **Bağımlılık:** İnternet bağlantısı gereklidir.

## Alternatif Scraper

Eğer Transfermarkt çalışmazsa, utils klasöründeki FBref scraper'ı kullanabilirsiniz:

**Dosya:** `utils/scraper.py`
**Özellik:** Selenium ile FBref ve Transfermarkt'tan veri çeker (daha yavaş ama daha güvenilir)

Kullanmak için `ai_service.py` içindeki scraper fonksiyonunu `utils/scraper.py` ile değiştirin.

## Test

Manuel test için:
```bash
curl -X POST http://localhost:5000/scrape_player \
  -H "Content-Type: application/json" \
  -d '{"name": "Cristiano Ronaldo"}'
```

Beklenen yanıt:
```json
{
  "status": "success",
  "FullName": "Cristiano Ronaldo",
  "TeamName": "Al Nassr",
  "Age": 39,
  "Nationality": "Portekiz",
  "CurrentMarketValue": 15000000,
  "Goals": 54,
  "Assists": 15,
  "MatchesPlayed": 72
}
```
