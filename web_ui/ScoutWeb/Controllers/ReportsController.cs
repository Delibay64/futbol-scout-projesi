using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutWeb.Models;

namespace ScoutWeb.Controllers
{
    // Geçici olarak Admin kontrolünü kaldırdım - test için
    // [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ScoutDbContext _context;

        public ReportsController(ScoutDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> AdminDashboard()
        {
            try
            {
                // ✅ VERİTABANI VIEW KULLANIMI: vw_PlayerDetailsTR (fn_EuroToTL fonksiyonu otomatik çalışır)
                var playerDetails = await _context.VwPlayerDetailsTR
                    .OrderByDescending(p => p.EuroValue)
                    .Take(20)
                    .Select(p => new PlayerDetailReport
                    {
                        FullName = p.FullName,
                        TeamName = p.TeamName,
                        Position = p.Position,
                        Age = p.Age ?? 0,
                        EuroValue = p.EuroValue ?? 0,
                        TLValue = p.TLValue ?? 0
                    })
                    .ToListAsync();

                // ✅ VERİTABANI VIEW KULLANIMI: vw_TopScorers (fn_GoalsPerMatch fonksiyonu otomatik çalışır)
                var topScorers = await _context.VwTopScorers
                    .OrderByDescending(s => s.Goals)
                    .Take(10)
                    .Select(s => new TopScorerReport
                    {
                        FullName = s.FullName,
                        Goals = s.Goals ?? 0,
                        Assists = s.Assists ?? 0,
                        GoalsPerMatch = s.GoalsPerMatch ?? 0
                    })
                    .ToListAsync();

                ViewBag.TopScorers = topScorers;

                return View(playerDetails);
            }
            catch (Exception ex)
            {
                // Hata mesajını TempData'ya ekle
                TempData["Error"] = $"Hata: {ex.Message}";

                // Boş liste ile view'i döndür
                ViewBag.TopScorers = new List<TopScorerReport>();
                return View(new List<PlayerDetailReport>());
            }
        }

        [HttpPost]
        // Geçici olarak Admin kontrolü kaldırıldı
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApplyRaise(int playerId, int percentage)
        {
            try
            {
                // ✅ STORED PROCEDURE KULLANIMI: sp_UpdateValue
                await _context.Database.ExecuteSqlRawAsync(
                    "CALL sp_UpdateValue({0}, {1})",
                    playerId,
                    percentage
                );

                TempData["Message"] = $"Oyuncu #{playerId} için %{percentage} zam uygulandı!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("AdminDashboard");
        }

        public async Task<IActionResult> TopScorers()
        {
            try
            {
                // ✅ VERİTABANI VIEW KULLANIMI: vw_TopScorers
                var scorers = await _context.VwTopScorers
                    .OrderByDescending(s => s.Goals)
                    .Take(20)
                    .ToListAsync();

                return View(scorers);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
                return View(new List<TopScorerView>());
            }
        }

        public async Task<IActionResult> YoungTalents()
        {
            try
            {
                // ✅ VERİTABANI VIEW KULLANIMI: vw_YoungTalents
                var talents = await _context.VwYoungTalents
                    .OrderByDescending(t => t.CurrentMarketValue)
                    .Take(20)
                    .ToListAsync();

                return View(talents);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
                return View(new List<YoungTalentView>());
            }
        }

        public async Task<IActionResult> TeamSummary()
        {
            try
            {
                // ✅ VERİTABANI VIEW KULLANIMI: vw_TeamSummary
                var teams = await _context.VwTeamSummary
                    .OrderByDescending(t => t.PlayerCount)
                    .Take(20)
                    .ToListAsync();

                return View(teams);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
                return View(new List<TeamSummaryView>());
            }
        }

        public async Task<IActionResult> PlayersTurkishLira()
        {
            try
            {
                // ✅ VERİTABANI VIEW KULLANIMI: vw_PlayerDetailsTR (fn_EuroToTL fonksiyonu içerir)
                var players = await _context.VwPlayerDetailsTR
                    .OrderByDescending(p => p.TLValue)
                    .Take(20)
                    .ToListAsync();

                return View(players);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
                return View(new List<PlayerDetailsTRView>());
            }
        }
    }
}