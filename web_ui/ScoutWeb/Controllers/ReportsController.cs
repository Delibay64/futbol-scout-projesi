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

                // ✅ SCOUT RAPORLARI - Onay Bekleyenler (Admin için)
                var pendingReports = await _context.Scoutreports
                    .Include(r => r.Player)
                    .Include(r => r.User)
                    .Where(r => !r.IsApproved) // Onaylanmamış raporlar
                    .OrderByDescending(r => r.ReportDate)
                    .ToListAsync();

                ViewBag.TopScorers = topScorers;
                ViewBag.PendingReports = pendingReports;

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

        // ✅ SCOUT RAPORLARI LİSTESİ (Onaylılar)
        public async Task<IActionResult> ScoutReport()
        {
            try
            {
                var isAdmin = HttpContext.Session.GetString("Username") == "admin";

                // Admin tüm raporları görür, normal kullanıcılar sadece onaylıları
                var reportsQuery = _context.Scoutreports
                    .Include(sr => sr.Player)
                        .ThenInclude(p => p!.Team)
                    .Include(sr => sr.User)
                    .AsQueryable();

                if (!isAdmin)
                {
                    // Normal kullanıcılar sadece onaylı raporları görür
                    reportsQuery = reportsQuery.Where(sr => sr.IsApproved);
                }

                var reports = await reportsQuery
                    .OrderByDescending(sr => sr.ReportDate)
                    .ToListAsync();

                // Admin için onay bekleyen raporları da gönder
                if (isAdmin)
                {
                    var pendingReports = reports.Where(r => !r.IsApproved).ToList();
                    ViewBag.PendingReports = pendingReports;
                    ViewBag.ApprovedReports = reports.Where(r => r.IsApproved).ToList();
                }

                ViewBag.IsAdmin = isAdmin;

                return View(reports);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
                return View(new List<Scoutreport>());
            }
        }

        // ✅ YENİ: RAPOR ONAYLAMA (Sadece Admin)
        [HttpPost]
        public async Task<IActionResult> ApproveReport(int reportId)
        {
            if (HttpContext.Session.GetString("Username") != "admin")
            {
                TempData["Error"] = "Bu işlem için yetkiniz yok!";
                return RedirectToAction("ScoutReport");
            }

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "CALL sp_ApproveScoutReport({0})",
                    reportId
                );

                TempData["Success"] = "✓ Rapor başarıyla onaylandı!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction("ScoutReport");
        }

        // ✅ YENİ: RAPOR REDDETME (Sadece Admin)
        [HttpPost]
        public async Task<IActionResult> RejectReport(int reportId)
        {
            if (HttpContext.Session.GetString("Username") != "admin")
            {
                TempData["Error"] = "Bu işlem için yetkiniz yok!";
                return RedirectToAction("ScoutReport");
            }

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "CALL sp_RejectScoutReport({0})",
                    reportId
                );

                TempData["Success"] = "✓ Rapor reddedildi ve silindi!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction("ScoutReport");
        }

        // --- FIX: SCOUT REPORT STORED PROCEDURES ---
        [HttpGet]
        public async Task<IActionResult> FixScoutReportProcedures()
        {
            if (HttpContext.Session.GetString("Username") != "admin")
            {
                TempData["Error"] = "Bu işlem için yetkiniz yok!";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // ÖNCE TÜM OLASI HATALILARI TEMİZLE (Gemini'nin farklı varyasyonları)
                await _context.Database.ExecuteSqlRawAsync(@"
DROP PROCEDURE IF EXISTS sp_approvescoutreport(integer);
DROP PROCEDURE IF EXISTS sp_ApprovescoutReport(integer);
DROP PROCEDURE IF EXISTS ""sp_ApproveScoutReport""(integer);
DROP PROCEDURE IF EXISTS sp_rejectscoutreport(integer);
DROP PROCEDURE IF EXISTS sp_RejectscoutReport(integer);
DROP PROCEDURE IF EXISTS ""sp_RejectScoutReport""(integer);");

                // 1. sp_ApproveScoutReport oluştur (DOĞRU TABLO ADI: scoutreports - küçük harf)
                await _context.Database.ExecuteSqlRawAsync(@"
CREATE OR REPLACE PROCEDURE sp_ApproveScoutReport(
    p_report_id INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE scoutreports
    SET is_approved = TRUE
    WHERE report_id = p_report_id;

    IF NOT FOUND THEN
        RAISE NOTICE 'Rapor ID % bulunamadı', p_report_id;
    ELSE
        RAISE NOTICE 'Scout raporu onaylandı: Rapor ID %', p_report_id;
    END IF;
END;
$$;");

                // 2. sp_RejectScoutReport oluştur (DOĞRU TABLO ADI: scoutreports - küçük harf)
                await _context.Database.ExecuteSqlRawAsync(@"
CREATE OR REPLACE PROCEDURE sp_RejectScoutReport(
    p_report_id INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM scoutreports
    WHERE report_id = p_report_id;

    IF NOT FOUND THEN
        RAISE NOTICE 'Rapor ID % bulunamadı', p_report_id;
    ELSE
        RAISE NOTICE 'Scout raporu reddedildi ve silindi: Rapor ID %', p_report_id;
    END IF;
END;
$$;");

                TempData["Success"] = "✓ Scout Report Stored Procedures başarıyla oluşturuldu!";
                return RedirectToAction("AdminDashboard");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
                return RedirectToAction("AdminDashboard");
            }
        }
    }
}