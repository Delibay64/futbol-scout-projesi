using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutWeb.Models;

namespace ScoutWeb.Controllers
{
    [Authorize]
    public class ScoutReportController : Controller
    {
        private readonly ScoutDbContext _context;

        public ScoutReportController(ScoutDbContext context)
        {
            _context = context;
        }

        // GET: ScoutReport
        public async Task<IActionResult> Index()
        {
            // Kullanıcının kendi raporlarını getir
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var reports = await _context.Scoutreports
                .Include(s => s.Player)
                .Include(s => s.User)
                .Where(s => s.User!.Username == username)
                .OrderByDescending(s => s.ReportDate)
                .ToListAsync();

            return View(reports);
        }

        // GET: ScoutReport/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Scoutreports
                .Include(s => s.Player)
                    .ThenInclude(p => p!.Team)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.ReportId == id);

            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // GET: ScoutReport/Create
        public async Task<IActionResult> Create(int? playerId)
        {
            if (playerId.HasValue)
            {
                var player = await _context.Players.FindAsync(playerId.Value);
                if (player != null)
                {
                    ViewBag.PlayerName = player.FullName;
                    ViewBag.PlayerId = playerId.Value;
                }
            }

            ViewBag.Players = await _context.Players
                .OrderBy(p => p.FullName)
                .ToListAsync();

            return View();
        }

        // POST: ScoutReport/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PlayerId,PredictedValue,Notes")] Scoutreport scoutreport)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı!";
                return RedirectToAction("Index");
            }

            try
            {
                // Stored Procedure kullanarak rapor oluştur
                object[] parameters = new object[]
                {
                    user.UserId,
                    scoutreport.PlayerId ?? 0,
                    scoutreport.PredictedValue ?? 0,
                    scoutreport.Notes ?? string.Empty
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "CALL sp_CreateScoutReport({0}, {1}, {2}, {3})",
                    parameters
                );

                TempData["Success"] = "✓ Scout raporu başarıyla oluşturuldu!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Rapor oluşturulurken hata: {ex.Message}";

                ViewBag.Players = await _context.Players
                    .OrderBy(p => p.FullName)
                    .ToListAsync();

                return View(scoutreport);
            }
        }

        // POST: ScoutReport/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var report = await _context.Scoutreports.FindAsync(id);
                if (report == null)
                {
                    TempData["Error"] = "Rapor bulunamadı!";
                    return RedirectToAction("Index");
                }

                // Sadece kendi raporunu silebilir
                var username = User.Identity?.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (report.UserId != user?.UserId)
                {
                    TempData["Error"] = "Bu raporu silme yetkiniz yok!";
                    return RedirectToAction("Index");
                }

                _context.Scoutreports.Remove(report);
                await _context.SaveChangesAsync();

                TempData["Success"] = "✓ Rapor başarıyla silindi!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Silme işlemi başarısız: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
