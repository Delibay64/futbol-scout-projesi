using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ScoutWeb.Models;
using ScoutWeb.Repositories;
using ScoutWeb.Services;
using ScoutWeb.BusinessLogic;
using ScoutWeb.Middleware;

var builder = WebApplication.CreateBuilder(args);

// --- 1. VERİTABANI BAĞLANTISI (Hata 2'nin Çözümü) ---
// Bu satır olmadan "Unable to resolve service ScoutDbContext" hatası alırsın.
builder.Services.AddDbContext<ScoutDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
    builder.Services.AddScoped<IPlayerService, PlayerService>();
    builder.Services.AddScoped<IValidationService, ValidationService>();

// --- SOA ENTEGRASYON: HttpClientFactory ---
// Node.js API, SOAP, REST çağrıları için gerekli
builder.Services.AddHttpClient();

// --- 2. SESSION YAPILANDIRMASI ---
// Session kullanmak için gerekli (Admin paneli role kontrolü için)
builder.Services.AddDistributedMemoryCache(); // Session için in-memory cache
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Session süresi
    options.Cookie.HttpOnly = true; // XSS koruması
    options.Cookie.IsEssential = true; // GDPR uyumlu
});

// --- 3. GİRİŞ SİSTEMİ (Hata 1'in Çözümü) ---
// Bu satır olmadan "No sign-in authentication handlers" hatası alırsın.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Giriş yapmamış kişi buraya gitsin
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

// Servisleri ekle (MVC yapısı için şart)
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Hata yönetimi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --- CROSS-CUTTING CONCERNS (6. KATMAN - SOA) ---
app.UseMiddleware<RequestLoggingMiddleware>(); // Tüm istekleri logla
app.UseMiddleware<ResponseCachingMiddleware>(); // GET istekleri cache'le

// --- SESSION'I AKTİF ET ---
app.UseSession(); // Session kullanımı için gerekli

// --- YETKİLENDİRME SIRASI (Çok Kritik!) ---
app.UseAuthentication(); // Önce: Kimsin? (Giriş kontrolü)
app.UseAuthorization();  // Sonra: Girebilir misin? (Yetki kontrolü)

// Rota Tanımlaması
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();