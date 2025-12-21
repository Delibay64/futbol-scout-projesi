using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace ScoutWeb.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ScoutDbContext _context;

        public ReportsController(ScoutDbContext context)
        {
            _context = context;
        }

        // 1. GET: /Reports (Yönetici Paneli - Geniş Özet)
        public async Task<IActionResult> Index()
        {
            var playerDetails = await _context.PlayerReports.ToListAsync();
            var topScorers = await _context.ScorerReports.OrderByDescending(x => x.Goals).ToListAsync();

            ViewBag.TopScorers = topScorers;
            
            return View(playerDetails);
        }

        // 2. GET: /Reports/TopScorers (Sadece Gol Krallığı Sayfası) - YENİ EKLENDİ
        public async Task<IActionResult> TopScorers()
        {
            // Gol sayısına göre çoktan aza sırala, ilk 20'yi al
            var topScorers = await _context.ScorerReports
                                           .OrderByDescending(x => x.Goals)
                                           .Take(20)
                                           .ToListAsync();
            return View(topScorers);
        }

        // 3. POST: Zam Yapma İşlemi (Gelişmiş Versiyon)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApplyRaise(int playerId, double percentage, string returnUrl)
        {
            try
            {
                // SQL Stored Procedure Çağır
                await _context.Database.ExecuteSqlRawAsync("CALL sp_UpdateValue({0}, {1})", playerId, percentage);
                
                TempData["Message"] = "✅ İşlem Başarılı: Oyuncunun piyasa değeri veritabanında güncellendi!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "❌ Hata: " + ex.Message;
            }

            // Eğer işlem Oyuncu Detay sayfasından yapıldıysa, oraya geri dön
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            // Yoksa rapor ana sayfasına dön
            return RedirectToAction(nameof(Index));
        }
    }
}