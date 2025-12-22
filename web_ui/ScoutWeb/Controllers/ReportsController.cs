using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutWeb.Models;

namespace ScoutWeb.Controllers
{
    [Authorize] // Giriş yapmış kullanıcılar erişebilir
    public class ReportsController : Controller
    {
        private readonly ScoutDbContext _context;

        public ReportsController(ScoutDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> AdminDashboard()
        {
            // Sadece "admin" kullanıcısı erişebilir
            if (HttpContext.Session.GetString("Username") != "admin")
            {
                TempData["Error"] = "Bu sayfaya erişim yetkiniz yok!";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // ✅ VERİTABANI VIEW KULLANIMI: vw_PlayerDetailsTR (fn_EuroToTL fonksiyonu otomatik çalışır)
                var playerDetails = await _context.VwPlayerDetailsTR
                    .OrderByDescending(p => p.EuroValue)
                    .Take(20)
                    .ToListAsync();

                // ✅ VERİTABANI VIEW KULLANIMI: vw_TopScorers (fn_GoalsPerMatch fonksiyonu otomatik çalışır)
                var topScorers = await _context.VwTopScorers
                    .OrderByDescending(s => s.Goals)
                    .Take(10)
                    .ToListAsync();

                ViewBag.TopScorers = topScorers;

                return View(playerDetails);
            }
            catch (Exception ex)
            {
                // Hata mesajını TempData'ya ekle
                TempData["Error"] = $"Hata: {ex.Message}";

                // Boş liste ile view'i döndür
                ViewBag.TopScorers = new List<TopScorerView>();
                return View(new List<PlayerDetailsTRView>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApplyRaise(int playerId, int percentage)
        {
            // Sadece "admin" kullanıcısı bu işlemi yapabilir
            // Cookie Authentication kullanıyoruz, User.Identity.Name ile kontrol
            if (User.Identity?.Name?.ToLower() != "admin")
            {
                TempData["Error"] = "Bu işlem için yetkiniz yok! Sadece admin kullanıcısı piyasa değeri değiştirebilir.";
                return RedirectToAction("Details", "Player", new { id = playerId });
            }

            try
            {
                // ✅ STORED PROCEDURE KULLANIMI: sp_UpdateValue
                await _context.Database.ExecuteSqlRawAsync(
                    "CALL sp_UpdateValue({0}, {1})",
                    playerId,
                    percentage
                );

                TempData["Success"] = $"✓ Oyuncu piyasa değeri %{percentage} oranında güncellendi!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction("Details", "Player", new { id = playerId });
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