using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutWeb.Models;

namespace ScoutWeb.ViewComponents
{
    /// <summary>
    /// VIEWCOMPONENT: En iyi golcüleri gösterir
    /// Kullanım: @await Component.InvokeAsync("TopScorers", new { count = 5 })
    /// </summary>
    public class TopScorersViewComponent : ViewComponent
    {
        private readonly ScoutDbContext _context;

        public TopScorersViewComponent(ScoutDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 5)
        {
            // VERİTABANI VIEW KULLANIMI: vw_TopScorers
            var topScorers = await _context.VwTopScorers
                .OrderByDescending(s => s.Goals)
                .Take(count)
                .ToListAsync();

            // count parametresini ViewBag ile aktar
            ViewBag.Count = count;

            return View(topScorers);
        }
    }
}
