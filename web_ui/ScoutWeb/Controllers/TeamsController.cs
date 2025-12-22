using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace ScoutWeb.Controllers
{
    /// <summary>
    /// 5. CONTROLLER: Takım yönetimi için tam CRUD işlemleri
    /// Index, Details, Create, Edit, Delete action'ları içerir
    /// </summary>
    [Authorize]
    public class TeamsController : Controller
    {
        private readonly ScoutDbContext _context;

        public TeamsController(ScoutDbContext context)
        {
            _context = context;
        }

        // GET: Teams
        // ACTION 1: Tüm takımları listele
        public async Task<IActionResult> Index()
        {
            var teams = await _context.Teams
                .Include(t => t.Players)
                .OrderBy(t => t.TeamName)
                .ToListAsync();

            // ViewBag kullanımı - Toplam takım sayısı
            ViewBag.TotalTeams = teams.Count;
            ViewBag.TotalPlayers = teams.Sum(t => t.Players.Count);

            return View(teams);
        }

        // GET: Teams/Details/5
        // ACTION 2: Takım detayları ve oyuncu listesi
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Takım ID'si belirtilmedi!";
                return RedirectToAction(nameof(Index));
            }

            var team = await _context.Teams
                .Include(t => t.Players)
                    .ThenInclude(p => p.Playerstats)
                .FirstOrDefaultAsync(m => m.TeamId == id);

            if (team == null)
            {
                TempData["Error"] = $"ID {id} numaralı takım bulunamadı!";
                return RedirectToAction(nameof(Index));
            }

            // ViewData kullanımı - Takım istatistikleri
            ViewData["PlayerCount"] = team.Players.Count;
            ViewData["TotalGoals"] = team.Players.Sum(p =>
                p.Playerstats.Sum(ps => ps.Goals ?? 0));
            ViewData["AverageAge"] = team.Players.Any() ?
                team.Players.Average(p => p.Age ?? 0) : 0;

            return View(team);
        }

        // GET: Teams/Create
        // ACTION 3: Yeni takım oluşturma formu
        public IActionResult Create()
        {
            ViewBag.Title = "Yeni Takım Ekle";
            return View();
        }

        // POST: Teams/Create
        // ACTION 4: Yeni takım kaydetme (CREATE işlemi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeamName,Country,LeagueName")] Team team)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(team);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"✓ {team.TeamName} takımı başarıyla eklendi!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Takım eklenirken hata oluştu: {ex.Message}";
                }
            }

            ViewBag.Title = "Yeni Takım Ekle";
            return View(team);
        }

        // GET: Teams/Edit/5
        // ACTION 5: Takım düzenleme formu (UPDATE işlemi - GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Takım ID'si belirtilmedi!";
                return RedirectToAction(nameof(Index));
            }

            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                TempData["Error"] = $"ID {id} numaralı takım bulunamadı!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Title = $"{team.TeamName} Takımını Düzenle";
            return View(team);
        }

        // POST: Teams/Edit/5
        // ACTION 6: Takım güncelleme (UPDATE işlemi - POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TeamId,TeamName,Country,LeagueName")] Team team)
        {
            if (id != team.TeamId)
            {
                TempData["Error"] = "Takım ID'si uyuşmuyor!";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(team);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"✓ {team.TeamName} takımı güncellendi!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(team.TeamId))
                    {
                        TempData["Error"] = "Takım artık mevcut değil!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Güncelleme hatası: {ex.Message}";
                }
            }

            ViewBag.Title = $"{team.TeamName} Takımını Düzenle";
            return View(team);
        }

        // POST: Teams/Delete/5
        // ACTION 7: Takım silme (DELETE işlemi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var team = await _context.Teams
                    .Include(t => t.Players)
                    .FirstOrDefaultAsync(t => t.TeamId == id);

                if (team == null)
                {
                    TempData["Error"] = "Takım bulunamadı!";
                    return RedirectToAction(nameof(Index));
                }

                // Takımda oyuncu varsa uyarı
                if (team.Players.Any())
                {
                    TempData["Error"] = $"{team.TeamName} takımında {team.Players.Count} oyuncu var. Önce oyuncuları silin veya başka takıma transfer edin!";
                    return RedirectToAction(nameof(Index));
                }

                string teamName = team.TeamName ?? "Bilinmeyen Takım";
                _context.Teams.Remove(team);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✓ {teamName} takımı başarıyla silindi!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Silme işlemi başarısız: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.TeamId == id);
        }
    }
}
