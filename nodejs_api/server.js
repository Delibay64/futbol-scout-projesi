const express = require('express');
const { Pool } = require('pg');
const cors = require('cors');
const soap = require('soap');
const fs = require('fs');
const app = express();
const PORT = 3000;

// Middleware
app.use(cors());
app.use(express.json());

// Static files (Vue.js frontend)
app.use(express.static('public'));

// PostgreSQL Bağlantısı
const pool = new Pool({
  host: 'localhost',
  database: 'ScoutDB',
  user: 'postgres',
  password: 'admin', // ← BURAYA KENDI ŞİFRENİ YAZ
  port: 5432
});

// API status endpoint (moved to /api)
app.get('/api', (req, res) => {
  res.json({
    status: 'success',
    message: 'Node.js API çalışıyor!',
    port: PORT,
    endpoints: {
      players: '/api/players',
      weather: '/api/weather/:city',
      exchange: '/api/exchange/:from/:to',
      soap: '/soap?wsdl'
    }
  });
});

// Vue.js frontend will be served from '/' by express.static

// Tüm oyuncuları getir
app.get('/api/players', async (req, res) => {
  try {
    const result = await pool.query(
      'SELECT player_id, full_name, position, age, current_market_value FROM players ORDER BY current_market_value DESC LIMIT 20'
    );
    res.json({ 
      status: 'success', 
      count: result.rows.length,
      players: result.rows 
    });
  } catch (error) {
    console.error('Hata:', error);
    res.status(500).json({ status: 'error', message: error.message });
  }
});

// ID'ye göre oyuncu getir
app.get('/api/players/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const result = await pool.query(
      `SELECT p.player_id, p.full_name, p.position, p.age, p.current_market_value, 
              t.team_name, t.league_name
       FROM players p
       LEFT JOIN teams t ON p.team_id = t.team_id
       WHERE p.player_id = $1`,
      [id]
    );
    
    if (result.rows.length === 0) {
      return res.status(404).json({ status: 'error', message: 'Oyuncu bulunamadı' });
    }
    
    res.json({ 
      status: 'success', 
      player: result.rows[0],
      source: 'Node.js REST API'
    });
  } catch (error) {
    console.error('Hata:', error);
    res.status(500).json({ status: 'error', message: error.message });
  }
});

// Yeni oyuncu ekle
app.post('/api/players', async (req, res) => {
  try {
    const { full_name, position, age, team_id } = req.body;
    
    const result = await pool.query(
      'INSERT INTO players (full_name, position, age, team_id) VALUES ($1, $2, $3, $4) RETURNING *',
      [full_name, position, age, team_id]
    );
    
    res.status(201).json({ 
      status: 'success', 
      message: 'Oyuncu eklendi',
      player: result.rows[0] 
    });
  } catch (error) {
    console.error('Hata:', error);
    res.status(500).json({ status: 'error', message: error.message });
  }
});

// Takımları getir
app.get('/api/teams', async (req, res) => {
  try {
    const result = await pool.query(
      'SELECT team_id, team_name, league_name FROM teams ORDER BY team_name LIMIT 50'
    );
    res.json({ 
      status: 'success', 
      count: result.rows.length,
      teams: result.rows 
    });
  } catch (error) {
    console.error('Hata:', error);
    res.status(500).json({ status: 'error', message: error.message });
  }
});
 
// HAZIR API: OpenWeatherMap - Hava Durumu (DEMO VERSION)
app.get('/api/weather/:city', async (req, res) => {
  try {
    const { city } = req.params;

    // SOA demonstration için mock data kullanıyoruz
    // Gerçek projelerde buraya kendi API key'inizi koyun
    const mockWeatherData = {
      'Istanbul': { temp: 15, feels_like: 13, humidity: 72, description: 'parçalı bulutlu' },
      'Ankara': { temp: 8, feels_like: 5, humidity: 65, description: 'açık' },
      'Izmir': { temp: 18, feels_like: 17, humidity: 68, description: 'güneşli' },
      'London': { temp: 12, feels_like: 10, humidity: 80, description: 'yağmurlu' },
      'Madrid': { temp: 20, feels_like: 19, humidity: 55, description: 'güneşli' }
    };

    const weatherData = mockWeatherData[city] || mockWeatherData['Istanbul'];

    res.json({
      status: 'success',
      name: city,
      main: {
        temp: weatherData.temp,
        feels_like: weatherData.feels_like,
        humidity: weatherData.humidity,
        pressure: 1013
      },
      weather: [{
        description: weatherData.description
      }],
      source: 'OpenWeatherMap API (Demo)',
      note: 'SOA entegrasyonu demonstration - Gerçek API için kendi key\'inizi kullanın'
    });
  } catch (error) {
    console.error('Hava durumu hatası:', error);
    res.status(500).json({ status: 'error', message: error.message });
  }
});

// Döviz Kuru API (DEMO VERSION)
app.get('/api/exchange/:from/:to', async (req, res) => {
  try {
    const { from, to } = req.params;

    // SOA demonstration için mock data
    const mockRates = {
      'EUR': { 'TRY': 36.85, 'USD': 1.08, 'GBP': 0.85 },
      'USD': { 'TRY': 34.12, 'EUR': 0.93, 'GBP': 0.79 },
      'GBP': { 'TRY': 43.20, 'EUR': 1.18, 'USD': 1.27 }
    };

    const fromUpper = from.toUpperCase();
    const toUpper = to.toUpperCase();

    if (!mockRates[fromUpper] || !mockRates[fromUpper][toUpper]) {
      return res.status(404).json({
        status: 'error',
        message: 'Döviz çifti bulunamadı. Desteklenen: EUR, USD, GBP'
      });
    }

    res.json({
      status: 'success',
      base_code: fromUpper,
      target_code: toUpper,
      conversion_rate: mockRates[fromUpper][toUpper],
      time_last_update_utc: new Date().toISOString(),
      source: 'ExchangeRate API (Demo)',
      note: 'SOA entegrasyonu demonstration - Gerçek API için kendi key\'inizi kullanın'
    });
  } catch (error) {
    console.error('Döviz kuru hatası:', error);
    res.status(500).json({ status: 'error', message: error.message });
  }
});

// SOAP SERVİSİ
const playerService = {
  PlayerService: {
    PlayerPort: {
      GetPlayer: async function(args) {
        try {
          const playerId = args.playerId;
          const result = await pool.query(
            `SELECT p.full_name, p.position, p.age, p.current_market_value
             FROM players p
             WHERE p.player_id = $1`,
            [playerId]
          );
          
          if (result.rows.length === 0) {
            return {
              fullName: 'Bulunamadı',
              position: 'N/A',
              age: 0,
              marketValue: '0'
            };
          }
          
          const player = result.rows[0];
          return {
            fullName: player.full_name || 'Bilinmiyor',
            position: player.position || 'N/A',
            age: player.age || 0,
            marketValue: player.current_market_value?.toString() || '0'
          };
        } catch (error) {
          console.error('SOAP Hatası:', error);
          return {
            fullName: 'Hata',
            position: 'N/A',
            age: 0,
            marketValue: '0'
          };
        }
      }
    }
  }
};

// SOAP endpoint'i başlat
const wsdlPath = './player.wsdl';
const xml = fs.readFileSync(wsdlPath, 'utf8');

// Servisi başlat (TEK SEFER)
app.listen(PORT, () => {
  console.log('\n' + '='.repeat(60));
  console.log('🚀 Node.js API ve SOAP Servisi Başlatıldı!');
  console.log('='.repeat(60));
  console.log(`✅ REST API: http://localhost:${PORT}`);
  console.log(`📊 Oyuncular: http://localhost:${PORT}/api/players`);
  console.log(`🌤️  Hava Durumu: http://localhost:${PORT}/api/weather/Istanbul`);
  console.log(`💱 Döviz Kuru: http://localhost:${PORT}/api/exchange/EUR/TRY`);

  // SOAP servisi başlat
  soap.listen(app, '/soap', playerService, xml, function(){
    console.log(`🧼 SOAP WSDL: http://localhost:${PORT}/soap?wsdl`);
    console.log('='.repeat(60) + '\n');
  });
});