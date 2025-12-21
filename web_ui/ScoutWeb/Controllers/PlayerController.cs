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

namespace ScoutWeb.Controllers
{
    [Authorize]
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

        // --- 1. LİSTELEME VE ARAMA SAYFASI ---
        public async Task<IActionResult> Index(string searchString)
        {
            // Servis katmanını kullan
            var players = await _playerService.GetPlayersAsync(searchString);
            return View(players);
        }

        // --- 2. DETAY SAYFASI ---
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Servis katmanını kullan
            var player = await _playerService.GetPlayerDetailsAsync(id.Value);

            if (player == null) return NotFound();

            return View(player);
        }

        // --- 3. YENİ OYUNCU EKLEME (SAYFAYI AÇ) ---
        public IActionResult Create()
        {
            ViewBag.Teams = _context.Teams.ToList();
            return View();
        }

        // --- 4. YENİ OYUNCU KAYDETME ---
        [HttpPost]
[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Player player, string? NewTeamName)
        {
            // Validation Service kullan
            if (!_validationService.ValidatePlayer(player, out string errorMessage))
            {
                ModelState.AddModelError("", errorMessage);
                ViewBag.Teams = _context.Teams.ToList();
                return View(player);
            }

            ModelState.Remove("TeamId");

            if (ModelState.IsValid)
            {
                // Servis katmanını kullan
                await _playerService.CreatePlayerAsync(player, NewTeamName);
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Teams = _context.Teams.ToList();
            return View(player);
        }

        // --- 5. YAPAY ZEKA FİYAT TAHMİNİ ---
        [HttpPost]
        public async Task<IActionResult> PredictPrice(int id)
        {
            var player = await _context.Players
                .Include(p => p.Team)
                .Include(p => p.Playerstats)
                .FirstOrDefaultAsync(p => p.PlayerId == id);

            if (player == null) return Json(new { status = "error", message = "Oyuncu bulunamadı." });

            var stats = player.Playerstats.FirstOrDefault();
            double macBasiSkor = 0;
            if (stats != null && stats.MatchesPlayed > 0)
            {
                macBasiSkor = (double)((stats.Goals ?? 0) + (stats.Assists ?? 0)) / (double)stats.MatchesPlayed;
            }

            var inputData = new
            {
                Oyuncu = player.FullName,
                Takim = player.Team?.TeamName ?? "Bilinmiyor",
                Lig = player.Team?.LeagueName ?? "Bilinmiyor",
                Ana_Mevki = player.Position ?? "Merkez Ortasaha",
                Ayak = "sag",
                Yas = player.Age ?? 25,
                Gol = stats?.Goals ?? 0,
                Asist = stats?.Assists ?? 0,
                Oynadigi_Sure_Dk = stats?.MinutesPlayed ?? 0,
                Mac_Sayisi = stats?.MatchesPlayed ?? 0,
                Sari_Kart = stats?.YellowCards ?? 0,
                Ilk_11 = stats?.MatchesPlayed ?? 0,
                Mac_Basi_Skor_Katkisi = macBasiSkor
            };

            try
            {
                using (var client = new HttpClient())
                {
                    var jsonContent = new StringContent(JsonSerializer.Serialize(inputData), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("http://localhost:5000/predict", jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return Content(result, "application/json");
                    }
                    else
                    {
                        return Json(new { status = "error", message = "AI servisine bağlanılamadı." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "Hata: " + ex.Message });
            }
        }

        // --- 6. SCRAPER KÖPRÜSÜ (VERİ ÇEKME) ---
        [HttpPost]
        public async Task<IActionResult> FetchPlayerData(string name)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var payload = new { name = name };
                    var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("http://localhost:5000/scrape_player", jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return Content(result, "application/json");
                    }
                    else
                    {
                        return Json(new { status = "error", message = "Python servisine ulaşılamadı veya oyuncu bulunamadı." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "Hata: " + ex.Message });
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

    // ViewModel (Controller DIŞINDA, namespace içinde)
    public class PlayerGrpcRequest
    {
        public int Id { get; set; }
    }
}
}