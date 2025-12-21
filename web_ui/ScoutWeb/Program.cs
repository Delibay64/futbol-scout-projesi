using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ScoutWeb.Models;

var builder = WebApplication.CreateBuilder(args);

// --- 1. VERİTABANI BAĞLANTISI (Hata 2'nin Çözümü) ---
// Bu satır olmadan "Unable to resolve service ScoutDbContext" hatası alırsın.
builder.Services.AddDbContext<ScoutDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 2. GİRİŞ SİSTEMİ (Hata 1'in Çözümü) ---
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

// --- 3. YETKİLENDİRME SIRASI (Çok Kritik!) ---
app.UseAuthentication(); // Önce: Kimsin? (Giriş kontrolü)
app.UseAuthorization();  // Sonra: Girebilir misin? (Yetki kontrolü)

// Rota Tanımlaması
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();