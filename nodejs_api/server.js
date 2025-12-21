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

// PostgreSQL Bağlantısı
const pool = new Pool({
  host: 'localhost',
  database: 'ScoutDB',
  user: 'postgres',
  password: 'admin', // ← BURAYA KENDI ŞİFRENİ YAZ
  port: 5432
});

// Test endpoint
app.get('/', (req, res) => {
  res.json({ 
    status: 'success', 
    message: 'Node.js API çalışıyor!',
    port: PORT 
  });
});

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
 
// HAZIR API: OpenWeatherMap - Hava Durumu
app.get('/api/weather/:city', async (req, res) => {
  try {
    const { city } = req.params;
    const apiKey = '8a3b5f7c9e2d1a6b4c8e0f2a5d9c7b1e'; // Ücretsiz demo key
    
    const fetch = (await import('node-fetch')).default;
    const response = await fetch(
      `https://api.openweathermap.org/data/2.5/weather?q=${city}&appid=${apiKey}&units=metric&lang=tr`
    );
    
    if (!response.ok) {
      return res.status(404).json({ 
        status: 'error', 
        message: 'Şehir bulunamadı' 
      });
    }
    
    const data = await response.json();
    
    res.json({
      status: 'success',
      city: data.name,
      country: data.sys.country,
      temperature: Math.round(data.main.temp),
      feels_like: Math.round(data.main.feels_like),
      description: data.weather[0].description,
      humidity: data.main.humidity,
      wind_speed: data.wind.speed,
      source: 'OpenWeatherMap API'
    });
  } catch (error) {
    console.error('Hava durumu hatası:', error);
    res.status(500).json({ status: 'error', message: error.message });
  }
});

// Döviz Kuru API
app.get('/api/exchange/:from/:to', async (req, res) => {
  try {
    const { from, to } = req.params;
    
    const fetch = (await import('node-fetch')).default;
    const response = await fetch(
      `https://api.exchangerate-api.com/v4/latest/${from.toUpperCase()}`
    );
    
    if (!response.ok) {
      return res.status(404).json({ 
        status: 'error', 
        message: 'Döviz bulunamadı' 
      });
    }
    
    const data = await response.json();
    const rate = data.rates[to.toUpperCase()];
    
    if (!rate) {
      return res.status(404).json({ 
        status: 'error', 
        message: `${to.toUpperCase()} dövizi bulunamadı` 
      });
    }
    
    res.json({
      status: 'success',
      from: from.toUpperCase(),
      to: to.toUpperCase(),
      rate: rate,
      date: data.date,
      source: 'ExchangeRate API'
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

app.listen(PORT, () => {
  console.log(`✅ Node.js API çalışıyor: http://localhost:${PORT}`);
  console.log(`📊 REST API: http://localhost:${PORT}/api/players`);
  
  // SOAP servisi başlat
  soap.listen(app, '/soap', playerService, xml, function(){
    console.log(`🧼 SOAP Servisi: http://localhost:${PORT}/soap?wsdl`);
  });
});

// Servisi başlat
app.listen(PORT, () => {
  console.log(`✅ Node.js API çalışıyor: http://localhost:${PORT}`);
  console.log(`📊 Test et: http://localhost:${PORT}/api/players`);
});