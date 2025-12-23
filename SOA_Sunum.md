# Futbol Scout Projesi - SOA Mimarisi Dokümantasyonu


## SOAP İLETİŞİM PROTOKOLÜ 

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



##  gRPC PROTOKOLÜ İLETİŞİMİ 

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

## NODE.JS API SERVİSİ 

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

##  HAZIR API KULLANIMI 

###  ExchangeRate API
**Tip:** REST API
**URL:** `https://api.exchangerate-api.com/v4/latest/`
**Dosya:** `nodejs_api/server.js` 

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
