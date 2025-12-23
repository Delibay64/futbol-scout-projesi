using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutWeb.Models;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Grpc.Net.Client;
using ScoutGrpcService;
using ScoutWeb.Services;
using ScoutWeb.Repositories;
using ScoutWeb.BusinessLogic;
using Npgsql;

namespace ScoutWeb.Controllers
{
    [Authorize]
    [Route("Player")]
    public class PlayerController : Controller
    {
        private readonly ScoutDbContext _context;
        private readonly IPlayerService _playerService;
        private readonly IValidationService _validationService;

        public PlayerController(
            ScoutDbContext context,
            IPlayerService playerService,
            IValidationService validationService)
        {
            _context = context;
            _playerService = playerService;
            _validationService = validationService;
        }


        // --- TOPLU DEĞER GÜNCELLEMESİ ---
        [HttpPost]
        [Route("BulkUpdateValues")]
        // Geçici olarak Admin kontrolü kaldırıldı
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkUpdateValues(int percentage)
        {
            try
            {
                // ✅ STORED PROCEDURE KULLANIMI: sp_UpdateValue
                // Tüm oyuncular için döngü ile SP çağır
                var players = await _context.Players
                    .Where(p => p.CurrentMarketValue != null)
                    .Select(p => p.PlayerId)
                    .ToListAsync();

                foreach (var playerId in players)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "CALL sp_UpdateValue({0}, {1})",
                        playerId,
                        percentage
                    );
                }

                TempData["Success"] = $"✓ {players.Count} oyuncunun değeri %{percentage} oranında güncellendi!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // --- HIZLI OYUNCU EKLEMESİ ---
        [HttpPost]
        // Geçici olarak Admin kontrolü kaldırıldı
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> QuickAddPlayer(string fullName, string position, int age, int teamId)
        {
            try
            {
                // ✅ STORED PROCEDURE KULLANIMI: sp_AddPlayerFast
                // Önce takım adını al
                var team = await _context.Teams.FindAsync(teamId);
                if (team == null)
                {
                    TempData["Error"] = "Takım bulunamadı!";
                    return RedirectToAction("Index");
                }

                // SP çağrısı - takım adı, oyuncu bilgileri ve başlangıç değeri
                await _context.Database.ExecuteSqlRawAsync(
                    "CALL sp_AddPlayerFast({0}, {1}, {2}, {3})",
                    fullName,
                    team.TeamName,
                    age,
                    100000 // Başlangıç piyasa değeri: 100,000 Euro
                );

                TempData["Success"] = $"✓ {fullName} başarıyla eklendi!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // --- 1. LİSTELEME VE ARAMA SAYFASI ---
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(string searchString)
        {
            // Servis katmanını kullan
            var players = await _playerService.GetPlayersAsync(searchString);
            return View(players);
        }

        [Route("Details/{id?}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var player = await _playerService.GetPlayerDetailsAsync(id.Value);

            if (player == null) return NotFound();

            // fn_EuroToTL FUNCTION KULLANIMI
            if (player.CurrentMarketValue.HasValue)
            {
                try
                {
                    var param = new NpgsqlParameter("@value", player.CurrentMarketValue.Value);
                    var tlValue = await _context.Database
                        .SqlQueryRaw<decimal>("SELECT fn_EuroToTL(@value) as Value", param)
                        .FirstOrDefaultAsync();

                    ViewBag.MarketValueTL = tlValue;
                }
                catch
                {
                    ViewBag.MarketValueTL = player.CurrentMarketValue.Value * 35;
                }
            }

            // fn_GoalsPerMatch FUNCTION KULLANIMI
            var stats = player.Playerstats.FirstOrDefault();
            if (stats != null && stats.MatchesPlayed > 0)
            {
                try
                {
                    var goalsParam = new NpgsqlParameter("@goals", stats.Goals ?? 0);
                    var matchesParam = new NpgsqlParameter("@matches", stats.MatchesPlayed);
                    var goalsPerMatch = await _context.Database
                        .SqlQueryRaw<decimal>("SELECT fn_GoalsPerMatch(@goals, @matches) as Value", goalsParam, matchesParam)
                        .FirstOrDefaultAsync();

                    ViewBag.GoalsPerMatch = goalsPerMatch;
                }
                catch
                {
                    ViewBag.GoalsPerMatch = (decimal)(stats.Goals ?? 0) / stats.MatchesPlayed;
                }
            }

            return View(player);
        }

        // --- 3. YENİ OYUNCU EKLEME (SAYFAYI AÇ) ---
        [Route("Create")]
        public IActionResult Create()
        {
            ViewBag.Teams = _context.Teams.ToList();
            return View();
        }

        // --- 4. YENİ OYUNCU KAYDETME ---
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Player player, string? NewTeamName)
        {
            try
            {
                // Validation Service kullan
                if (!_validationService.ValidatePlayer(player, out string errorMessage))
                {
                    ModelState.AddModelError("", errorMessage);
                    ViewBag.Teams = _context.Teams.ToList();
                    return View(player);
                }

                ModelState.Remove("TeamId");
                ModelState.Remove("Team");
                ModelState.Remove("Playerstats");
                ModelState.Remove("Scoutreports");

                if (ModelState.IsValid)
                {
                    // Servis katmanını kullan
                    await _playerService.CreatePlayerAsync(player, NewTeamName);
                    TempData["Success"] = $"✓ {player.FullName} başarıyla eklendi!";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Teams = _context.Teams.ToList();
                return View(player);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
                ViewBag.Teams = _context.Teams.ToList();
                return View(player);
            }
        }

        // --- 5. YAPAY ZEKA FİYAT TAHMİNİ (gRPC → Python ML) ---
        [HttpPost]
        public async Task<IActionResult> PredictPrice(int id)
        {
            try
            {
                // HTTP/2 desteğini aktif et
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                // gRPC kanalı oluştur (HTTP/2 ile)
                var channel = Grpc.Net.Client.GrpcChannel.ForAddress(
                    "http://localhost:5001",
                    new Grpc.Net.Client.GrpcChannelOptions
                    {
                        HttpHandler = new System.Net.Http.SocketsHttpHandler
                        {
                            EnableMultipleHttp2Connections = true,
                            KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                            KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                            ConnectTimeout = TimeSpan.FromSeconds(10)
                        }
                    });

                var client = new ScoutGrpcService.PlayerService.PlayerServiceClient(channel);

                // gRPC'ye tahmin isteği gönder
                var request = new ScoutGrpcService.PredictionRequest
                {
                    PlayerId = id
                };

                var response = await client.PredictValueAsync(request);

                await channel.ShutdownAsync();

                return Json(new
                {
                    status = response.Status,
                    tahmini_deger = response.PredictedValue,
                    message = response.Message
                });
            }
            catch (Grpc.Core.RpcException ex)
            {
                return Json(new
                {
                    status = "error",
                    message = $"gRPC Hatası: {ex.Status.Detail}",
                    hint = "gRPC servisi (port 5001) veya Python ML servisi (port 5000) çalışmıyor olabilir."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "error",
                    message = $"Bağlantı hatası: {ex.Message}",
                    hint = "BASLAT_GRPC.bat ve BASLAT_ML_SERVICE.bat dosyalarını çalıştırın."
                });
            }
        }

        // --- 6. SCRAPER KÖPRÜSÜ (Node.js API üzerinden) ---
        [HttpPost]
        public async Task<IActionResult> FetchPlayerData(string name)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);

                    // Node.js scraper endpoint'ini çağır
                    var response = await client.GetAsync($"http://localhost:3000/api/scraper/search/{Uri.EscapeDataString(name)}");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResult = await response.Content.ReadAsStringAsync();
                        var scraperData = JsonSerializer.Deserialize<JsonElement>(jsonResult);

                        // Sonuç var mı kontrol et
                        if (scraperData.GetProperty("count").GetInt32() == 0)
                        {
                            return Json(new
                            {
                                status = "error",
                                message = $"'{name}' için oyuncu bulunamadı."
                            });
                        }

                        // İlk sonucu al
                        var player = scraperData.GetProperty("results")[0];

                        // C# formatına dönüştür
                        return Json(new
                        {
                            status = "success",
                            FullName = player.GetProperty("name").GetString(),
                            TeamName = player.GetProperty("team").GetString(),
                            Position = player.GetProperty("position").GetString(),
                            Age = player.GetProperty("age").GetInt32(),
                            CurrentMarketValue = player.GetProperty("marketValue").GetInt32(),
                            Nationality = player.TryGetProperty("nationality", out var nat) ? nat.GetString() : "Bilinmiyor",
                            Goals = 0,
                            Assists = 0,
                            MatchesPlayed = 0,
                            MinutesPlayed = 0
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            status = "error",
                            message = "Scraper servisi yanıt vermedi. Node.js API (port 3000) çalışmıyor olabilir.",
                            hint = "START_NODEJS_API.bat dosyasını çalıştırın."
                        });
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                return Json(new
                {
                    status = "error",
                    message = "Node.js API erişilebilir değil.",
                    hint = "START_NODEJS_API.bat dosyasını çalıştırın.",
                    details = ex.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "error",
                    message = $"Hata: {ex.Message}"
                });
            }
        }

        // --- 7. NODE.JS API ÇAĞRISI ---
        [HttpPost]
        public async Task<IActionResult> GetPlayerViaNodeApi([FromBody] PlayerGrpcRequest request)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync($"http://localhost:3000/api/players/{request.Id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Content(content, "application/json");
                }

                return Json(new { status = "error", message = "Node.js API'den veri alınamadı" });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

        // --- 9. SOAP SERVİS ÇAĞRISI ---
        [HttpPost]
        [Route("Player/GetPlayerViaSOAP")]
        public async Task<IActionResult> GetPlayerViaSOAP([FromBody] PlayerGrpcRequest request)
        {
            try
            {
                using var client = new HttpClient();
                
                var soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <GetPlayer xmlns=""http://localhost:3000/wsdl"">
      <playerId>{request.Id}</playerId>
    </GetPlayer>
  </soap:Body>
</soap:Envelope>";

                var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "GetPlayer");
                
                var response = await client.PostAsync("http://localhost:3000/soap", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    
                    string fullName = "Bilinmiyor";
                    string position = "N/A";
                    string marketValue = "0";
                    
                    try
                    {
                        // tns:fullName veya fullName formatını destekle
                        if (result.Contains("fullName>"))
                        {
                            var start = result.IndexOf("fullName>") + 9;
                            var end = result.IndexOf("</", start);
                            if (end > start)
                            {
                                fullName = result.Substring(start, end - start);
                            }
                        }
                        
                        if (result.Contains("position>"))
                        {
                            var start = result.IndexOf("position>") + 9;
                            var end = result.IndexOf("</", start);
                            if (end > start)
                            {
                                position = result.Substring(start, end - start);
                            }
                        }
                        
                        if (result.Contains("marketValue>"))
                        {
                            var start = result.IndexOf("marketValue>") + 12;
                            var end = result.IndexOf("</", start);
                            if (end > start)
                            {
                                marketValue = result.Substring(start, end - start);
                            }
                        }
                    }
                    catch
                    {
                        // Parse hatası
                    }
                    
                    return Json(new
                    {
                        status = "success",
                        player = new
                        {
                            fullName = fullName,
                            position = position,
                            marketValue = marketValue,
                            source = "SOAP Servisi"
                        }
                    });
                }

                return Json(new { status = "error", message = "SOAP servisine bağlanılamadı" });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

        // --- YENİ: OYUNCU İSTATİSTİKLERİNİ GÜNCELLE (sp_UpdatePlayerStats) ---
        [HttpPost]
        public async Task<IActionResult> UpdatePlayerStats(
            int playerId,
            string season,
            int matches,
            int goals,
            int assists,
            int yellowCards = 0,
            int redCards = 0,
            int minutes = 0)
        {
            try
            {
                // ✅ STORED PROCEDURE KULLANIMI: sp_UpdatePlayerStats
                await _context.Database.ExecuteSqlRawAsync(
                    "CALL sp_UpdatePlayerStats({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
                    playerId, season, matches, goals, assists, yellowCards, redCards, minutes
                );

                TempData["Success"] = $"✓ {season} sezonu istatistikleri güncellendi!";
                return RedirectToAction("Details", new { id = playerId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
                return RedirectToAction("Details", new { id = playerId });
            }
        }

        // --- YENİ: SCOUT RAPORU OLUŞTUR (sp_CreateScoutReport) ---
        [HttpPost]
        public async Task<IActionResult> CreateScoutReport(
            int playerId,
            decimal predictedValue,
            string notes)
        {
            try
            {
                // Kullanıcı ID'sini session'dan al
                var username = HttpContext.Session.GetString("Username");
                if (string.IsNullOrEmpty(username))
                {
                    return Json(new { status = "error", message = "Giriş yapmalısınız!" });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    return Json(new { status = "error", message = "Kullanıcı bulunamadı!" });
                }

                // ✅ STORED PROCEDURE KULLANIMI: sp_CreateScoutReport
                await _context.Database.ExecuteSqlRawAsync(
                    "CALL sp_CreateScoutReport({0}, {1}, {2}, {3})",
                    user.UserId, playerId, predictedValue, notes ?? ""
                );

                return Json(new {
                    status = "success",
                    message = "Scout raporu başarıyla oluşturuldu!",
                    predictedValue = predictedValue
                });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

        // --- DELETE İŞLEMİ ---
        [HttpPost]
        [Route("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var player = await _context.Players
                    .Include(p => p.Playerstats)
                    .Include(p => p.Scoutreports)
                    .FirstOrDefaultAsync(p => p.PlayerId == id);

                if (player == null)
                {
                    TempData["Error"] = "Oyuncu bulunamadı!";
                    return RedirectToAction("Index");
                }

                string playerName = player.FullName ?? "Bilinmeyen Oyuncu";

                // Önce ilişkili kayıtları sil (Foreign Key Constraint için)

                // 1. PlayerPriceLogs silme
                var priceLogs = await _context.PriceLogs
                    .Where(pl => pl.PlayerId == id)
                    .ToListAsync();
                if (priceLogs.Any())
                {
                    _context.PriceLogs.RemoveRange(priceLogs);
                }

                // 2. Playerstats silme
                if (player.Playerstats != null && player.Playerstats.Any())
                {
                    _context.Playerstats.RemoveRange(player.Playerstats);
                }

                // 3. Scoutreports silme
                if (player.Scoutreports != null && player.Scoutreports.Any())
                {
                    _context.Scoutreports.RemoveRange(player.Scoutreports);
                }

                // Sonra oyuncuyu sil
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✓ {playerName} başarıyla silindi!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Silme işlemi başarısız: {ex.InnerException?.Message ?? ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // --- ML TAHMİN ENDPOİNTİ (gRPC üzerinden) ---
        [HttpPost]
        [Route("PredictPrice")]
        public async Task<IActionResult> PredictPriceViaGrpc(int id)
        {
            try
            {
                // HTTP/2 desteğini aktif et
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                // gRPC kanalı oluştur (HTTP/2 ile)
                var channel = Grpc.Net.Client.GrpcChannel.ForAddress(
                    "http://localhost:5001",
                    new Grpc.Net.Client.GrpcChannelOptions
                    {
                        HttpHandler = new System.Net.Http.SocketsHttpHandler
                        {
                            EnableMultipleHttp2Connections = true,
                            KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                            KeepAlivePingTimeout = TimeSpan.FromSeconds(30)
                        }
                    });

                var client = new ScoutGrpcService.PlayerService.PlayerServiceClient(channel);

                // gRPC'ye tahmin isteği gönder
                var request = new ScoutGrpcService.PredictionRequest
                {
                    PlayerId = id
                };

                var response = await client.PredictValueAsync(request);

                await channel.ShutdownAsync();

                return Json(new
                {
                    status = response.Status,
                    tahmini_deger = response.PredictedValue,
                    message = response.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "error",
                    message = ex.Message
                });
            }
        }
    }

    // ViewModel (Controller DIŞINDA, namespace içinde)
    public class PlayerGrpcRequest
    {
        public int Id { get; set; }
    }
}