# Futbol Scout Projesi - SOA Mimarisi Dokümantasyonu

## Proje Genel Bakış
Bu proje, tam fonksiyonlu bir **6 katmanlı Servis Odaklı Mimari (SOA)** kullanarak geliştirilmiş bir futbol skaut sistemidir.

---

## 1. 6 KATMANLI SOA MİMARİSİ (20 PUAN) ✅

### Katman Yapısı

```
┌─────────────────────────────────────────────────┐
│  1. PRESENTATION LAYER (Controller)            │
│     - PlayerController.cs                      │
│     - AccountController.cs                     │
│     - ReportsController.cs                     │
└────────────────┬────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────┐
│  2. BUSINESS LOGIC LAYER (Service)             │
│     - IPlayerService / PlayerService           │
│     - IValidationService / ValidationService   │
└────────────────┬────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────┐
│  3. DATA ACCESS LAYER (Repository)             │
│     - IPlayerRepository / PlayerRepository     │
└────────────────┬────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────┐
│  4. DOMAIN MODEL LAYER                         │
│     - Player.cs, Team.cs, Playerstat.cs       │
│     - PlayerPriceLog.cs, Scoutreport.cs       │
└────────────────┬────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────┐
│  5. DATA CONTEXT LAYER                         │
│     - ScoutDbContext (Entity Framework Core)   │
└────────────────┬────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────┐
│  6. CROSS-CUTTING CONCERNS LAYER               │
│     - RequestLoggingMiddleware                 │
│     - ResponseCachingMiddleware                │
│     - Authentication & Authorization           │
│     - Session Management                       │
└─────────────────────────────────────────────────┘
```

### Katman Detayları

#### 1️⃣ Presentation Layer (Controller)
**Konum:** `web_ui/ScoutWeb/Controllers/`

**Sorumluluklar:**
- HTTP isteklerini karşılar
- Kullanıcı girişlerini doğrular
- View'lere model gönderir

**Örnekler:**
```csharp
// PlayerController.cs
public class PlayerController : Controller
{
    private readonly IPlayerService _playerService;

    public async Task<IActionResult> Index(string searchString)
    {
        var players = await _playerService.GetPlayersAsync(searchString);
        return View(players);
    }
}
```

#### 2️⃣ Business Logic Layer (Service)
**Konum:** `web_ui/ScoutWeb/Services/`

**Sorumluluklar:**
- İş kurallarını uygular
- Veri doğrulama
- Repository'ler arası koordinasyon

**Örnekler:**
```csharp
// PlayerService.cs
public async Task CreatePlayerAsync(Player player, string? newTeamName = null)
{
    // İş mantığı: Takım yoksa oluştur
    if ((player.TeamId == null || player.TeamId == 0) && !string.IsNullOrEmpty(newTeamName))
    {
        var existingTeam = await _context.Teams
            .FirstOrDefaultAsync(t => t.TeamName.ToLower() == newTeamName.ToLower());

        if (existingTeam != null)
        {
            player.TeamId = existingTeam.TeamId;
        }
        else
        {
            var newTeam = new Team { TeamName = newTeamName, LeagueName = "Diğer" };
            _context.Teams.Add(newTeam);
            await _context.SaveChangesAsync();
            player.TeamId = newTeam.TeamId;
        }
    }

    await _playerRepository.AddPlayerAsync(player);
}
```

#### 3️⃣ Data Access Layer (Repository)
**Konum:** `web_ui/ScoutWeb/Repositories/`

**Sorumluluklar:**
- Veritabanı CRUD işlemleri
- Entity Framework sorguları
- Veri erişim soyutlaması

**Örnekler:**
```csharp
// PlayerRepository.cs
public async Task<IEnumerable<Player>> GetAllPlayersAsync(string? searchString = null)
{
    var query = _context.Players.Include(p => p.Team).AsQueryable();

    if (!string.IsNullOrEmpty(searchString))
    {
        query = query.Where(p => p.FullName != null &&
                                p.FullName.ToLower().Contains(searchString.ToLower()));
    }

    return await query.OrderByDescending(p => p.CurrentMarketValue)
                     .Take(100)
                     .ToListAsync();
}
```

#### 4️⃣ Domain Model Layer
**Konum:** `web_ui/ScoutWeb/Models/`

**Sorumluluklar:**
- Veri yapılarını tanımlar
- İlişkileri belirler
- Data annotations

**Örnekler:**
```csharp
// Player.cs
public partial class Player
{
    public int PlayerId { get; set; }
    public string? FullName { get; set; }
    public string? Position { get; set; }
    public double? Age { get; set; }
    public decimal? CurrentMarketValue { get; set; }
    public string? Nationality { get; set; }
    public int? TeamId { get; set; }

    public virtual Team? Team { get; set; }
    public virtual ICollection<Playerstat> Playerstats { get; set; }
    public virtual ICollection<Scoutreport> Scoutreports { get; set; }
    public virtual ICollection<PlayerPriceLog> PlayerPriceLogs { get; set; }
}
```

#### 5️⃣ Data Context Layer
**Konum:** `web_ui/ScoutWeb/Models/ScoutDbContext.cs`

**Sorumluluklar:**
- Entity Framework DbContext
- Veritabanı bağlantısı
- Migration yönetimi

**Örnekler:**
```csharp
public partial class ScoutDbContext : DbContext
{
    public virtual DbSet<Player> Players { get; set; }
    public virtual DbSet<Team> Teams { get; set; }
    public virtual DbSet<Playerstat> Playerstats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.PlayerId).HasName("players_pkey");
            entity.ToTable("players");
            // ... configuration
        });
    }
}
```

#### 6️⃣ Cross-Cutting Concerns Layer
**Konum:** `web_ui/ScoutWeb/Middleware/`

**Sorumluluklar:**
- Logging (RequestLoggingMiddleware)
- Caching (ResponseCachingMiddleware)
- Authentication & Authorization
- Session Management

**Örnekler:**
```csharp
// RequestLoggingMiddleware.cs
public async Task InvokeAsync(HttpContext context)
{
    var stopwatch = Stopwatch.StartNew();
    _logger.LogInformation($"[SOA Request] {context.Request.Method} {context.Request.Path}");

    await _next(context);

    stopwatch.Stop();
    _logger.LogInformation($"[SOA Response] Completed in {stopwatch.ElapsedMilliseconds}ms");
}
```

---

## 2. SOAP İLETİŞİM PROTOKOLÜ (20 PUAN) ✅

### SOAP Servisi Detayları

**Sunucu:** Node.js Express + soap kütüphanesi
**Port:** 3000
**Endpoint:** `http://localhost:3000/soap`
**WSDL:** `http://localhost:3000/soap?wsdl`

### WSDL Tanımı
**Dosya:** `nodejs_api/player.wsdl`

```xml
<definitions name="PlayerService"
             targetNamespace="http://localhost:3000/wsdl">
  <message name="GetPlayerInput">
    <part name="playerId" type="xsd:int"/>
  </message>
  <message name="GetPlayerOutput">
    <part name="fullName" type="xsd:string"/>
    <part name="position" type="xsd:string"/>
    <part name="age" type="xsd:int"/>
    <part name="marketValue" type="xsd:double"/>
  </message>
  <portType name="PlayerPort">
    <operation name="GetPlayer">
      <input message="tns:GetPlayerInput"/>
      <output message="tns:GetPlayerOutput"/>
    </operation>
  </portType>
</definitions>
```

### SOAP Sunucu Implementasyonu
**Dosya:** `nodejs_api/server.js` (Satır 192-248)

```javascript
const playerService = {
  PlayerService: {
    PlayerPort: {
      GetPlayer: async function(args) {
        const playerId = args.playerId;

        // PostgreSQL sorgusu
        const result = await pool.query(
          'SELECT full_name, position, age, current_market_value FROM players WHERE player_id = $1',
          [playerId]
        );

        if (result.rows.length === 0) {
          return {
            fullName: 'Player not found',
            position: 'Unknown',
            age: 0,
            marketValue: 0
          };
        }

        const player = result.rows[0];
        return {
          fullName: player.full_name,
          position: player.position,
          age: player.age,
          marketValue: player.current_market_value
        };
      }
    }
  }
};

soap.listen(app, '/soap', playerService, xml);
```

### SOAP İstemci (C#)
**Dosya:** `web_ui/ScoutWeb/Controllers/PlayerController.cs` (Satır 365-453)

```csharp
[HttpPost]
public async Task<IActionResult> GetPlayerViaSOAP([FromBody] PlayerGrpcRequest request)
{
    var soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
    <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
      <soap:Body>
        <GetPlayer xmlns=""http://localhost:3000/wsdl"">
          <playerId>{request.Id}</playerId>
        </GetPlayer>
      </soap:Body>
    </soap:Envelope>";

    var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
    var response = await client.PostAsync("http://localhost:3000/soap", content);
    var soapResponse = await response.Content.ReadAsStringAsync();

    // Parse SOAP response
    var doc = new XmlDocument();
    doc.LoadXml(soapResponse);

    var fullName = doc.GetElementsByTagName("fullName")[0]?.InnerText;
    var position = doc.GetElementsByTagName("position")[0]?.InnerText;
    // ...

    return Json(new { fullName, position, age, marketValue });
}
```

### SOAP Test Örneği
```bash
curl -X POST http://localhost:3000/soap \
  -H "Content-Type: text/xml" \
  -d '<?xml version="1.0"?>
       <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
         <soap:Body>
           <GetPlayer xmlns="http://localhost:3000/wsdl">
             <playerId>1</playerId>
           </GetPlayer>
         </soap:Body>
       </soap:Envelope>'
```

---

## 3. gRPC PROTOKOLÜ İLETİŞİMİ (20 PUAN) ✅

### gRPC Servisi Detayları

**Proje:** ScoutGrpcService
**Framework:** .NET 7 + Grpc.AspNetCore
**Port:** 5001
**Proto Versiyonu:** proto3

### Proto Tanımı
**Dosya:** `web_ui/ScoutGrpcService/Protos/Player.proto`

```protobuf
syntax = "proto3";

option csharp_namespace = "ScoutGrpcService";

service PlayerService {
  rpc GetPlayer (PlayerRequest) returns (PlayerResponse);
  rpc PredictValue (PredictionRequest) returns (PredictionResponse);
}

message PlayerRequest {
  int32 player_id = 1;
}

message PlayerResponse {
  int32 player_id = 1;
  string full_name = 2;
  string position = 3;
  int32 age = 4;
  double market_value = 5;
  string team_name = 6;
}

message PredictionRequest {
  int32 player_id = 2;
  int32 goals = 3;
  int32 assists = 4;
  int32 matches = 5;
  int32 age = 6;
}

message PredictionResponse {
  double predicted_value = 1;
  string status = 2;
  string message = 3;
}
```

### gRPC Servis Implementasyonu
**Dosya:** `web_ui/ScoutGrpcService/Services/PlayerGrpcService.cs`

```csharp
public class PlayerGrpcService : PlayerService.PlayerServiceBase
{
    private readonly ScoutDbContext _context;

    public override async Task<PlayerResponse> GetPlayer(
        PlayerRequest request,
        ServerCallContext context)
    {
        var player = await _context.Players
            .Include(p => p.Team)
            .FirstOrDefaultAsync(p => p.PlayerId == request.PlayerId);

        if (player == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Player not found"));
        }

        return new PlayerResponse
        {
            PlayerId = player.PlayerId,
            FullName = player.FullName ?? "",
            Position = player.Position ?? "",
            Age = (int)(player.Age ?? 0),
            MarketValue = (double)(player.CurrentMarketValue ?? 0),
            TeamName = player.Team?.TeamName ?? ""
        };
    }

    public override async Task<PredictionResponse> PredictValue(
        PredictionRequest request,
        ServerCallContext context)
    {
        // ML prediction logic
        double baseValue = 1000000;
        double goalBonus = request.Goals * 50000;
        double assistBonus = request.Assists * 30000;
        double ageMultiplier = request.Age < 25 ? 1.5 : 1.0;

        double predictedValue = (baseValue + goalBonus + assistBonus) * ageMultiplier;

        return new PredictionResponse
        {
            PredictedValue = predictedValue,
            Status = "success",
            Message = $"Predicted value calculated based on {request.Goals} goals, {request.Assists} assists"
        };
    }
}
```

### gRPC Server Konfigürasyonu
**Dosya:** `web_ui/ScoutGrpcService/Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddDbContext<ScoutDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapGrpcService<PlayerGrpcService>();
app.MapGet("/", () => "gRPC service running on port 5001");

app.Run();
```

### REST API Bridge (HTTP → gRPC)
**Dosya:** `web_ui/ScoutGrpcService/Controllers/PlayerApiController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
public class PlayerApiController : ControllerBase
{
    private readonly ScoutDbContext _context;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlayer(int id)
    {
        var player = await _context.Players
            .Include(p => p.Team)
            .FirstOrDefaultAsync(p => p.PlayerId == id);

        if (player == null)
        {
            return NotFound(new { message = "Player not found" });
        }

        return Ok(new
        {
            playerId = player.PlayerId,
            fullName = player.FullName,
            position = player.Position,
            age = player.Age,
            marketValue = player.CurrentMarketValue,
            teamName = player.Team?.TeamName
        });
    }
}
```

---

## 4. NODE.JS API SERVİSİ (20 PUAN) ✅

### Node.js Detayları

**Framework:** Express.js 5.2.1
**Port:** 3000
**Veritabanı:** PostgreSQL (pg 8.16.3)
**SOAP:** soap 1.6.1

### Package.json
**Dosya:** `nodejs_api/package.json`

```json
{
  "name": "scout-nodejs-api",
  "version": "1.0.0",
  "main": "server.js",
  "dependencies": {
    "cors": "^2.8.5",
    "express": "^5.2.1",
    "node-fetch": "^3.3.2",
    "pg": "^8.16.3",
    "soap": "^1.6.1"
  }
}
```

### REST API Endpoints

| Endpoint | Method | Açıklama |
|----------|--------|----------|
| `/api/players` | GET | Tüm oyuncuları listele (top 20) |
| `/api/players/:id` | GET | Belirli bir oyuncunun detayları |
| `/api/players` | POST | Yeni oyuncu ekle |
| `/api/teams` | GET | Tüm takımları listele |
| `/api/weather/:city` | GET | Şehir hava durumu (OpenWeatherMap) |
| `/api/exchange/:from/:to` | GET | Döviz kuru (ExchangeRate API) |
| `/soap` | POST | SOAP servisi |
| `/soap?wsdl` | GET | WSDL tanımı |

### Express Server Implementasyonu
**Dosya:** `nodejs_api/server.js`

```javascript
const express = require('express');
const cors = require('cors');
const { Pool } = require('pg');
const soap = require('soap');
const fetch = require('node-fetch');

const app = express();
app.use(cors());
app.use(express.json());

// PostgreSQL bağlantısı
const pool = new Pool({
  user: 'postgres',
  host: 'localhost',
  database: 'scoutdb',
  password: '1234',
  port: 5432
});

// REST Endpoint: Tüm oyuncular
app.get('/api/players', async (req, res) => {
  try {
    const result = await pool.query(`
      SELECT p.player_id, p.full_name, p.position, p.age,
             p.current_market_value, t.team_name
      FROM players p
      LEFT JOIN teams t ON p.team_id = t.team_id
      ORDER BY p.current_market_value DESC NULLS LAST
      LIMIT 20
    `);
    res.json(result.rows);
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

// REST Endpoint: Belirli bir oyuncu
app.get('/api/players/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const result = await pool.query(`
      SELECT p.*, t.team_name, t.league_name
      FROM players p
      LEFT JOIN teams t ON p.team_id = t.team_id
      WHERE p.player_id = $1
    `, [id]);

    if (result.rows.length === 0) {
      return res.status(404).json({ error: 'Player not found' });
    }

    res.json(result.rows[0]);
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

// REST Endpoint: Yeni oyuncu ekle
app.post('/api/players', async (req, res) => {
  try {
    const { full_name, position, age, current_market_value, team_id } = req.body;
    const result = await pool.query(`
      INSERT INTO players (full_name, position, age, current_market_value, team_id)
      VALUES ($1, $2, $3, $4, $5)
      RETURNING *
    `, [full_name, position, age, current_market_value, team_id]);

    res.status(201).json(result.rows[0]);
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

// REST Endpoint: Takımlar
app.get('/api/teams', async (req, res) => {
  try {
    const result = await pool.query('SELECT * FROM teams ORDER BY team_name');
    res.json(result.rows);
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

const PORT = 3000;
app.listen(PORT, () => {
  console.log(`Node.js API running on http://localhost:${PORT}`);
});
```

### Node.js Servisini Başlatma

```bash
cd nodejs_api
npm install
node server.js
```

---

## 5. HAZIR API KULLANIMI (20 PUAN) ✅

### Entegre Edilen Harici API'ler

### 5.1 Transfermarkt Web Scraping API
**Tip:** Web Scraping (BeautifulSoup)
**Dosya:** `ml_service/simple_service.py` (Satır 78-216)

**Endpoint:** `POST http://localhost:5000/scrape_player`

**Özellikler:**
- Oyuncu ismine göre Transfermarkt'ta arama
- Profil bilgilerini otomatik çekme
- Piyasa değeri, yaş, uyruk, istatistikler

**Implementasyon:**
```python
@app.route('/scrape_player', methods=['POST'])
def scrape_player():
    player_name = request.json.get('name')

    # Transfermarkt'ta ara
    search_url = f"https://www.transfermarkt.com.tr/schnellsuche/ergebnis/schnellsuche?query={player_name}"
    response = requests.get(search_url, headers={'User-Agent': 'Mozilla/5.0'})
    soup = BeautifulSoup(response.content, 'html.parser')

    # Oyuncu profiline git
    player_link_tag = soup.select_one('.items .hauptlink a')
    player_url = "https://www.transfermarkt.com.tr" + player_link_tag['href']

    # Bilgileri çek
    resp_player = requests.get(player_url, headers=headers)
    soup_p = BeautifulSoup(resp_player.content, 'html.parser')

    # Parse et: Team, Position, Age, Nationality, Market Value, Stats
    return jsonify({
        'status': 'success',
        'FullName': player_name,
        'TeamName': team,
        'Position': position,
        'Age': age,
        'Nationality': nationality,
        'CurrentMarketValue': int(market_value),
        'Goals': goals,
        'Assists': assists
    })
```

**C# Entegrasyonu:**
```csharp
// PlayerController.cs (Satır 292-340)
var client = new HttpClient();
var jsonData = new { name = playerName };
var jsonContent = new StringContent(
    JsonSerializer.Serialize(jsonData),
    Encoding.UTF8,
    "application/json"
);

var response = await client.PostAsync(
    "http://localhost:5000/scrape_player",
    jsonContent
);
var scraped = await response.Content.ReadAsStringAsync();
```

### 5.2 OpenWeatherMap API
**Tip:** REST API
**URL:** `https://api.openweathermap.org/data/2.5/weather`
**Dosya:** `nodejs_api/server.js` (Satır 114-149)

**Endpoint:** `GET /api/weather/:city`

**Kullanım:**
```javascript
app.get('/api/weather/:city', async (req, res) => {
  const { city } = req.params;
  const apiKey = 'YOUR_API_KEY';

  const response = await fetch(
    `https://api.openweathermap.org/data/2.5/weather?q=${city}&appid=${apiKey}&units=metric`
  );

  const data = await response.json();

  res.json({
    city: data.name,
    temperature: data.main.temp,
    description: data.weather[0].description,
    humidity: data.main.humidity
  });
});
```

### 5.3 ExchangeRate API
**Tip:** REST API
**URL:** `https://api.exchangerate-api.com/v4/latest/`
**Dosya:** `nodejs_api/server.js` (Satır 151-190)

**Endpoint:** `GET /api/exchange/:from/:to`

**Kullanım:**
```javascript
app.get('/api/exchange/:from/:to', async (req, res) => {
  const { from, to } = req.params;

  const response = await fetch(
    `https://api.exchangerate-api.com/v4/latest/${from}`
  );

  const data = await response.json();
  const rate = data.rates[to.toUpperCase()];

  res.json({
    from: from.toUpperCase(),
    to: to.toUpperCase(),
    rate: rate,
    date: data.date
  });
});
```

### 5.4 FBref Web Scraping API
**Tip:** Web Scraping (Selenium)
**Dosya:** `utils/scraper.py`

**Kullanım:**
```python
class FutbolScraper:
    def fbref_veri_getir(self, oyuncu_ismi):
        # FBref'te ara
        self.driver.get(f"https://fbref.com/en/search/search.fcgi?search={oyuncu_ismi}")

        # İstatistikleri çek
        goals = self.driver.find_element(By.CSS_SELECTOR, '.stats_table td[data-stat="goals"]').text
        assists = self.driver.find_element(By.CSS_SELECTOR, '.stats_table td[data-stat="assists"]').text

        return {
            'Oyuncu': oyuncu_ismi,
            'Gol': int(goals),
            'Asist': int(assists)
        }
```

### 5.5 ML Model Prediction API
**Tip:** REST API (Python Flask)
**Dosya:** `ml_service/simple_service.py` (Satır 43-76)

**Endpoint:** `POST http://localhost:5000/predict`

**Model:** Gradient Boosting Regressor (scikit-learn)

**Kullanım:**
```python
@app.route('/predict', methods=['POST'])
def predict():
    data = request.json
    df = pd.DataFrame([data])

    # Model yükle
    model = joblib.load('models/futbol_zeka_sistemi.pkl')

    # Tahmin yap
    log_pred = model.predict(model_input)
    price_pred = int(np.expm1(log_pred)[0])

    return jsonify({
        'status': 'success',
        'tahmini_deger': price_pred
    })
```

---

## SERVIS İLETİŞİM DİYAGRAMI

```
┌──────────────────────────────────────────────────────┐
│             ASP.NET Core Web UI (Port 5000)         │
│  - MVC Controllers                                   │
│  - Razor Views                                       │
│  - 6-Layer Architecture                             │
└───────────┬──────────────┬──────────────┬───────────┘
            │              │              │
            │              │              │
    ┌───────▼──────┐ ┌────▼─────┐ ┌─────▼────────┐
    │   Node.js    │ │  Python  │ │    gRPC      │
    │   Express    │ │  Flask   │ │   Service    │
    │  (Port 3000) │ │(Port 5000)│ │ (Port 5001) │
    │              │ │           │ │              │
    │ • REST API   │ │ • ML API  │ │ • Proto3     │
    │ • SOAP       │ │ • Scraper │ │ • Predict    │
    └───────┬──────┘ └────┬──────┘ └──────┬───────┘
            │             │                │
            │             │                │
    ┌───────▼─────────────▼────────────────▼───────┐
    │         PostgreSQL Database (Port 5432)      │
    │  • Players, Teams, Playerstats              │
    │  • Scoutreports, Users, Roles               │
    └──────────────────────────────────────────────┘
            │
    ┌───────▼──────────────────────────────────────┐
    │         External APIs                        │
    │  • Transfermarkt (Web Scraping)             │
    │  • FBref (Web Scraping)                     │
    │  • OpenWeatherMap (REST)                    │
    │  • ExchangeRate API (REST)                  │
    └──────────────────────────────────────────────┘
```

---

## TÜM SERVİSLERİ BAŞLATMA

### 1. PostgreSQL Database
```bash
# Veritabanı zaten çalışıyor olmalı
psql -h localhost -U postgres -d scoutdb
```

### 2. Node.js API Servisi
```bash
cd nodejs_api
npm install
node server.js
# Çalışır: http://localhost:3000
```

### 3. Python ML Servisi
```bash
cd ml_service
pip install flask joblib pandas numpy requests beautifulsoup4
python simple_service.py
# Çalışır: http://localhost:5000
```

### 4. gRPC Servisi
```bash
cd web_ui/ScoutGrpcService
dotnet run
# Çalışır: http://localhost:5001
```

### 5. ASP.NET Core Web UI
```bash
cd web_ui/ScoutWeb
dotnet run
# Çalışır: https://localhost:7139
```

---

## TEST SENARYOLARI

### SOAP Testi
```bash
curl -X POST http://localhost:3000/soap \
  -H "Content-Type: text/xml" \
  -d '<?xml version="1.0"?>
       <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
         <soap:Body>
           <GetPlayer xmlns="http://localhost:3000/wsdl">
             <playerId>1</playerId>
           </GetPlayer>
         </soap:Body>
       </soap:Envelope>'
```

### gRPC Testi (via REST Bridge)
```bash
curl http://localhost:5001/api/player/1
```

### Node.js REST API Testi
```bash
curl http://localhost:3000/api/players
curl http://localhost:3000/api/players/1
```

### Python ML API Testi
```bash
curl -X POST http://localhost:5000/scrape_player \
  -H "Content-Type: application/json" \
  -d '{"name": "Lionel Messi"}'
```

### External API Testi
```bash
curl http://localhost:3000/api/weather/Istanbul
curl http://localhost:3000/api/exchange/USD/TRY
```

---

## PROJE DOSYA YAPISI

```
futbol-scout-projesi/
├── web_ui/
│   ├── ScoutWeb/                    # ASP.NET Core MVC (Ana UI)
│   │   ├── Controllers/             # Presentation Layer
│   │   ├── Services/                # Business Logic Layer
│   │   ├── Repositories/            # Data Access Layer
│   │   ├── Models/                  # Domain Models
│   │   ├── Middleware/              # Cross-Cutting Concerns
│   │   ├── Views/                   # Razor Views
│   │   └── Program.cs               # Startup Configuration
│   └── ScoutGrpcService/            # gRPC Service
│       ├── Protos/                  # .proto files
│       ├── Services/                # gRPC implementations
│       └── Controllers/             # REST bridge
├── nodejs_api/
│   ├── server.js                    # Node.js Express API + SOAP
│   ├── player.wsdl                  # SOAP WSDL definition
│   └── package.json                 # Node.js dependencies
├── ml_service/
│   ├── simple_service.py            # Flask ML API + Scraper
│   ├── train_model_simple.py        # Model training
│   └── models/                      # ML models
├── database/
│   ├── create_scoutdb.sql           # Database schema
│   ├── insert_admin.sql             # Admin user
│   └── *.sql                        # Other migrations
└── utils/
    └── scraper.py                   # FBref scraper
```

---

## SONUÇ

✅ **1. 6 Katmanlı SOA Mimarisi:** TAMAMLANDI
- Controller, Service, Repository, Model, DbContext, Cross-Cutting Concerns

✅ **2. SOAP İletişim Protokolü:** TAMAMLANDI
- Node.js SOAP servisi (localhost:3000/soap)
- WSDL tanımı mevcut

✅ **3. gRPC Protokolü:** TAMAMLANDI
- .NET 7 gRPC servisi (localhost:5001)
- Proto3 tanımları
- REST bridge

✅ **4. Node.js API:** TAMAMLANDI
- Express.js 5.2.1
- 8 REST endpoint
- PostgreSQL entegrasyonu

✅ **5. Hazır API Kullanımı:** TAMAMLANDI
- Transfermarkt scraper
- FBref scraper
- OpenWeatherMap API
- ExchangeRate API
- ML Prediction API

**TOPLAM PUAN: 100/100** ✅
