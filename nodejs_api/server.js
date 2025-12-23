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

// SCRAPER: Transfermarkt'tan oyuncu ara
app.get('/api/scraper/search/:playerName', async (req, res) => {
  try {
    const { playerName } = req.params;
    console.log(`🔍 Scraper: "${playerName}" aranıyor...`);

    // Mock data (gerçek scraper için puppeteer/cheerio kullanılabilir)
    const mockPlayers = [
      { name: 'Cristiano Ronaldo', team: 'Al-Nassr', league: 'Saudi Pro League', position: 'Forvet', age: 39, marketValue: 15000000, nationality: 'Portekiz' },
      { name: 'Lionel Messi', team: 'Inter Miami', league: 'MLS', position: 'Sağ Kanat', age: 36, marketValue: 35000000, nationality: 'Arjantin' },
      { name: 'Kylian Mbappé', team: 'Real Madrid', league: 'LaLiga', position: 'Sol Kanat', age: 25, marketValue: 180000000, nationality: 'Fransa' }
    ];

    const results = mockPlayers.filter(p => p.name.toLowerCase().includes(playerName.toLowerCase()));

    res.json({
      status: 'success',
      query: playerName,
      count: results.length,
      results: results,
      source: 'Mock Data (Demo)',
      note: 'Gerçek scraper için Transfermarkt entegrasyonu eklenebilir'
    });
  } catch (error) {
    console.error('Scraper hatası:', error);
    res.status(500).json({ status: 'error', message: error.message });
  }
});

// Döviz Kuru API (GERÇEK API KEY - CANLI VERİ)
app.get('/api/exchange/:from/:to', async (req, res) => {
  try {
    const { from, to } = req.params;
    const API_KEY = 'd1894d2d40ca978d85376110';
    const fromUpper = from.toUpperCase();
    const toUpper = to.toUpperCase();

    console.log(`💱 Döviz Kuru İsteği: ${fromUpper}/${toUpper}`);

    // Gerçek API'den veri çek (HER ZAMAN GÜNCEL)
    const fetch = (await import('node-fetch')).default;
    const apiUrl = `https://v6.exchangerate-api.com/v6/${API_KEY}/pair/${fromUpper}/${toUpper}`;

    console.log(`🌐 API Çağrısı: ${apiUrl}`);

    const response = await fetch(apiUrl);

    if (!response.ok) {
      console.error(`❌ API Hatası: ${response.status} ${response.statusText}`);
      return res.status(500).json({
        status: 'error',
        message: `ExchangeRate API hatası: ${response.statusText}`,
        hint: 'API key kontrol edin veya quota aşıldı'
      });
    }

    const data = await response.json();

    if (data.result === 'error') {
      console.error(`❌ API Yanıt Hatası: ${data['error-type']}`);
      return res.status(400).json({
        status: 'error',
        message: data['error-type'],
        hint: 'Döviz çifti geçersiz veya desteklenmiyor'
      });
    }

    const currentRate = data.conversion_rate;
    const lastUpdate = data.time_last_update_utc;

    console.log(`✅ Güncel Kur: 1 ${fromUpper} = ${currentRate} ${toUpper}`);
    console.log(`🕒 Son Güncelleme: ${lastUpdate}`);

    res.json({
      status: 'success',
      base_code: data.base_code,
      target_code: data.target_code,
      conversion_rate: currentRate,
      time_last_update_utc: lastUpdate,
      time_next_update_utc: data.time_next_update_utc,
      source: 'ExchangeRate API',
      type: 'CANLI VERİ - Gerçek Zamanlı',
      note: `Son güncelleme: ${new Date(lastUpdate).toLocaleString('tr-TR')}`
    });
  } catch (error) {
    console.error('💥 Döviz kuru hatası:', error);
    res.status(500).json({
      status: 'error',
      message: error.message,
      hint: 'İnternet bağlantısı veya API servisi kontrol edin'
    });
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
      },

      // YENİ: Döviz kuru doğrulama servisi (GERÇEK ZAMANLI)
      ValidateExchangeRate: async function(args) {
        try {
          const { fromCurrency, toCurrency, providedRate } = args;
          const API_KEY = 'd1894d2d40ca978d85376110';
          const fromUpper = fromCurrency.toUpperCase();
          const toUpper = toCurrency.toUpperCase();
          const providedNum = parseFloat(providedRate);

          console.log(`\n🔍 SOAP DOĞRULAMA İSTEĞİ:`);
          console.log(`   Döviz Çifti: ${fromUpper}/${toUpper}`);
          console.log(`   Gönderilen Oran: ${providedNum}`);

          // Gerçek API'den güncel veriyi çek
          const fetch = (await import('node-fetch')).default;
          const apiUrl = `https://v6.exchangerate-api.com/v6/${API_KEY}/pair/${fromUpper}/${toUpper}`;

          console.log(`🌐 SOAP için API Çağrısı: ${apiUrl}`);

          const response = await fetch(apiUrl);

          if (!response.ok) {
            console.error(`❌ SOAP API Hatası: ${response.status} ${response.statusText}`);
            return {
              isValid: false,
              message: `API çağrısı başarısız: ${response.statusText}`,
              actualRate: '0',
              difference: '0',
              percentageDiff: '0',
              status: 'error',
              timestamp: new Date().toISOString(),
              lastUpdate: 'Bilinmiyor'
            };
          }

          const data = await response.json();

          if (data.result === 'error') {
            console.error(`❌ SOAP API Yanıt Hatası: ${data['error-type']}`);
            return {
              isValid: false,
              message: `API hatası: ${data['error-type']}`,
              actualRate: '0',
              difference: '0',
              percentageDiff: '0',
              status: 'error',
              timestamp: new Date().toISOString(),
              lastUpdate: 'Bilinmiyor'
            };
          }

          const actualRate = data.conversion_rate;
          const difference = Math.abs(actualRate - providedNum);
          const percentageDiff = ((difference / actualRate) * 100).toFixed(2);
          const tolerance = actualRate * 0.01; // %1 tolerans
          const isValid = difference <= tolerance;
          const lastUpdate = data.time_last_update_utc;

          console.log(`\n💱 CANLI KUR BİLGİSİ:`);
          console.log(`   Güncel Oran: 1 ${fromUpper} = ${actualRate} ${toUpper}`);
          console.log(`   Gönderilen: ${providedNum}`);
          console.log(`   Fark: ${difference.toFixed(4)} (${percentageDiff}%)`);
          console.log(`   Tolerans: ±${tolerance.toFixed(4)} (%1)`);
          console.log(`   Durum: ${isValid ? '✅ GEÇERLİ' : '❌ GEÇERSİZ'}`);
          console.log(`   Son Güncelleme: ${lastUpdate}`);
          console.log(`   Türkçe Zaman: ${new Date(lastUpdate).toLocaleString('tr-TR')}\n`);

          return {
            isValid: isValid,
            message: isValid
              ? `✅ Kur doğrulandı! Fark sadece ${percentageDiff}% (Tolerans: %1)`
              : `❌ Kur güncel değil! Fark ${percentageDiff}% (Tolerans aşıldı)`,
            actualRate: actualRate.toString(),
            difference: difference.toFixed(4),
            percentageDiff: percentageDiff,
            status: 'success',
            timestamp: new Date().toISOString(),
            lastUpdate: new Date(lastUpdate).toLocaleString('tr-TR'),
            providedRate: providedNum.toString(),
            currencyPair: `${fromUpper}/${toUpper}`,
            source: 'ExchangeRate API (CANLI VERİ)'
          };
        } catch (error) {
          console.error('💥 SOAP Doğrulama Hatası:', error);
          return {
            isValid: false,
            message: `Hata: ${error.message}`,
            actualRate: '0',
            difference: '0',
            percentageDiff: '0',
            status: 'error',
            timestamp: new Date().toISOString(),
            lastUpdate: 'Bilinmiyor'
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
  console.log(`💱 Döviz Kuru: http://localhost:${PORT}/api/exchange/EUR/TRY`);

  // SOAP servisi başlat
  soap.listen(app, '/soap', playerService, xml, function(){
    console.log(`🧼 SOAP WSDL: http://localhost:${PORT}/soap?wsdl`);
    console.log('='.repeat(60) + '\n');
  });
});