# Python Scraper Entegrasyonu

## Nasıl Çalışır?

Player/Create sayfasında yeni oyuncu eklerken, oyuncu ismini yazıp **"🌐 Verileri Çek"** butonuna tıkladığınızda:

1. C# Controller (`PlayerController.ScrapePlayer`) Python script'ini çalıştırır
2. Python script (`scraper_cli.py`) FBref ve Transfermarkt'tan veri çeker:
   - **FBref**: Gol, asist, maç sayısı, oynadığı dakika
   - **Transfermarkt**: Piyasa değeri
3. Veriler JSON formatında C#'a döner
4. Form alanları otomatik doldurulur

## Gereksinimler

### Python Paketleri

```bash
pip install selenium webdriver-manager
```

### Chrome Driver
Selenium otomatik olarak Chrome driver'ı indirir (webdriver-manager sayesinde).

## Dosya Yapısı

```
utils/
├── scraper.py          # Ana scraper sınıfı (FutbolScraper)
└── scraper_cli.py      # CLI wrapper (C# için JSON çıktı verir)

web_ui/ScoutWeb/
└── Controllers/
    └── PlayerController.cs  # ScrapePlayer endpoint (satır 467)

web_ui/ScoutWeb/Views/Player/
└── Create.cshtml       # Scraper butonu ve JavaScript kodu
```

## Test

### Manuel Test (Terminal)

```bash
cd utils
python scraper_cli.py "Cristiano Ronaldo"
```

Çıktı:
```json
{
  "full_name": "Cristiano Ronaldo",
  "age": 39,
  "position": "Forvet",
  "nationality": "Bilinmiyor",
  "team_name": "Al Nassr",
  "league_name": "Saudi Pro League",
  "current_market_value": 15000000,
  "stats": {
    "goals": 54,
    "assists": 15,
    "matches_played": 72,
    "minutes_played": 6300,
    "yellow_cards": 0,
    "red_cards": 0
  }
}
```

### Web Üzerinden Test

1. `http://localhost:5199/Player/Create` adresine git
2. **Ad Soyad** alanına "Erling Haaland" yaz
3. **🌐 Verileri Çek** butonuna tıkla
4. Form alanları otomatik doldurulacak

## Sorun Giderme

### Python bulunamadı
```bash
# Windows'ta Python yolu kontrol et
where python
```

Eğer `python` komutu çalışmıyorsa, `PlayerController.cs` satır 473'te:
```csharp
var pythonPath = "python3"; // veya "C:\\Python311\\python.exe"
```

### Selenium hatası
```bash
# Selenium ve webdriver-manager'ı yeniden yükle
pip uninstall selenium webdriver-manager -y
pip install selenium webdriver-manager
```

### Script path hatası
`PlayerController.cs` satır 474'te script path'i kontrol et:
```csharp
var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "utils", "scraper_cli.py");
```

Eğer çalışmazsa mutlak path kullan:
```csharp
var scriptPath = @"C:\Users\odeve\git\futbol-scout-projesi\utils\scraper_cli.py";
```

## Özellikler

✅ FBref'ten oyuncu istatistikleri
✅ Transfermarkt'tan piyasa değeri
✅ Otomatik takım eşleştirme
✅ Yeni takım otomatik ekleme
✅ Headless browser (arka planda çalışır)
✅ JSON çıktı (C# entegrasyonu)

## Sınırlamalar

⚠️ **Milliyet bilgisi**: FBref'ten çekilemiyor, "Bilinmiyor" olarak kaydediliyor
⚠️ **Pozisyon**: Scraper'dan gelen pozisyon listede yoksa "Forvet" olarak ayarlanıyor
⚠️ **Hız**: İlk çalıştırmada Chrome driver indirileceği için yavaş olabilir
⚠️ **Rate Limiting**: Çok fazla istek gönderirseniz FBref veya Transfermarkt IP'nizi engelleyebilir

## Gelecek Geliştirmeler

- [ ] Milliyet bilgisini FBref'ten çekme
- [ ] Pozisyon bilgisini daha akıllı eşleştirme
- [ ] Caching sistemi (aynı oyuncu için tekrar scrape etmeme)
- [ ] Progress bar (scraping sırasında yükleniyor göstergesi)
- [ ] Multiple data sources (SofaScore, WhoScored)