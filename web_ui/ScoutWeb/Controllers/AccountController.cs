using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutWeb.Models;
using System.Security.Claims;

namespace ScoutWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ScoutDbContext _context;

        public AccountController(ScoutDbContext context)
        {
            _context = context;
        }

        // --- GİRİŞ YAP (LOGIN) ---
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kullanıcıyı kullanıcı adına göre bul
                var user = await _context.Users
                    .Include(u => u.Role) // Rolünü de çek (Admin mi User mı?)
                    .FirstOrDefaultAsync(u => u.Username == model.Username);

                // Kullanıcı bulundu ve şifre doğru mu kontrol et (BCrypt ile)
                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    // Kimlik Kartını Oluştur (Claims)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "User"), // Rolü yoksa User say
                        new Claim("UserId", user.UserId.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Giriş Yap (Cookie Ver)
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    // Session'a kullanıcı bilgilerini kaydet (Admin paneli kontrolü için)
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Role", user.Role?.RoleName ?? "Viewer");
                    HttpContext.Session.SetInt32("UserId", user.UserId);

                    return RedirectToAction("Index", "Home"); // Ana sayfaya git
                }

                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı!");
            }
            return View(model);
        }

        // --- KAYIT OL (REGISTER) ---
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kullanıcı adı zaten var mı?
                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("", "Bu kullanıcı adı zaten alınmış.");
                    return View(model);
                }

                // Varsayılan Rolü Bul (User) - Yoksa ilk rolü al
                var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User") 
                               ?? await _context.Roles.FirstOrDefaultAsync();

                // Yeni Kullanıcı Oluştur
                var newUser = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password), // BCrypt ile şifrele
                    RoleId = defaultRole?.RoleId ?? 1,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // --- ÇIKIŞ YAP (LOGOUT) ---
        public async Task<IActionResult> Logout()
        {
            // Session'ı temizle
            HttpContext.Session.Clear();

            // Cookie authentication'dan çıkış yap
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }
    }
}