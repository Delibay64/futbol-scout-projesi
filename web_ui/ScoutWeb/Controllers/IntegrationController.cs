using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ScoutWeb.Controllers
{
    /// <summary>
    /// SOA ENTEGRASYON CONTROLLER
    /// - Node.js REST API
    /// - SOAP Servisi
    /// - gRPC Servisi
    /// - Hazır API'ler (OpenWeatherMap, ExchangeRate)
    /// </summary>
    [Authorize]
    public class IntegrationController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IntegrationController> _logger;

        // Node.js API Base URL
        private const string NODE_API_URL = "http://localhost:3000";

        public IntegrationController(IHttpClientFactory httpClientFactory, ILogger<IntegrationController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // ==========================================
        // ANA SAYFA - Tüm Entegrasyonları Göster
        // ==========================================
        public IActionResult Index()
        {
            ViewBag.Title = "SOA Entegrasyonları";
            return View();
        }

        // ==========================================
        // NODE.JS REST API DEMO
        // ==========================================
        public async Task<IActionResult> NodeApiDemo()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                // Node.js API'den oyuncu listesi çek
                var response = await client.GetAsync($"{NODE_API_URL}/api/players");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<JsonElement>(json);

                    ViewBag.ApiStatus = "success";
                    ViewBag.ApiResponse = json;
                    ViewBag.Players = data.GetProperty("players");
                }
                else
                {
                    ViewBag.ApiStatus = "error";
                    ViewBag.ErrorMessage = $"HTTP {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.ApiStatus = "error";
                ViewBag.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Node.js API hatası");
            }

            return View();
        }

        // ==========================================
        // HAZIR API DEMO - Hava Durumu + Döviz
        // ==========================================
        public async Task<IActionResult> ExternalApisDemo()
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                // 1. Hava Durumu API (OpenWeatherMap via Node.js)
                var weatherResponse = await client.GetAsync($"{NODE_API_URL}/api/weather/Istanbul");
                if (weatherResponse.IsSuccessStatusCode)
                {
                    var weatherJson = await weatherResponse.Content.ReadAsStringAsync();
                    ViewBag.WeatherData = JsonSerializer.Deserialize<JsonElement>(weatherJson);
                    ViewBag.WeatherStatus = "success";
                }
                else
                {
                    ViewBag.WeatherStatus = "error";
                }
            }
            catch (Exception ex)
            {
                ViewBag.WeatherStatus = "error";
                ViewBag.WeatherError = ex.Message;
            }

            try
            {
                // 2. Döviz Kuru API (ExchangeRate via Node.js)
                var exchangeResponse = await client.GetAsync($"{NODE_API_URL}/api/exchange/EUR/TRY");
                if (exchangeResponse.IsSuccessStatusCode)
                {
                    var exchangeJson = await exchangeResponse.Content.ReadAsStringAsync();
                    ViewBag.ExchangeData = JsonSerializer.Deserialize<JsonElement>(exchangeJson);
                    ViewBag.ExchangeStatus = "success";
                }
                else
                {
                    ViewBag.ExchangeStatus = "error";
                }
            }
            catch (Exception ex)
            {
                ViewBag.ExchangeStatus = "error";
                ViewBag.ExchangeError = ex.Message;
            }

            return View();
        }

        // ==========================================
        // SOAP DEMO - Oyuncu Bilgisi Çek
        // ==========================================
        public async Task<IActionResult> SoapDemo()
        {
            ViewBag.SoapEndpoint = $"{NODE_API_URL}/soap?wsdl";

            // SOAP servisi için basit HTTP POST (gerçek SOAP client yerine)
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                // SOAP XML request oluştur
                var soapEnvelope = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <GetPlayer xmlns=""http://localhost:3000/wsdl"">
      <playerId>1</playerId>
    </GetPlayer>
  </soap:Body>
</soap:Envelope>";

                var content = new StringContent(soapEnvelope, System.Text.Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "GetPlayer");

                var response = await client.PostAsync($"{NODE_API_URL}/soap", content);

                if (response.IsSuccessStatusCode)
                {
                    var soapResponse = await response.Content.ReadAsStringAsync();
                    ViewBag.SoapStatus = "success";
                    ViewBag.SoapResponse = soapResponse;
                }
                else
                {
                    ViewBag.SoapStatus = "error";
                    ViewBag.ErrorMessage = $"HTTP {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.SoapStatus = "error";
                ViewBag.ErrorMessage = ex.Message;
                _logger.LogError(ex, "SOAP hatası");
            }

            return View();
        }

        // ==========================================
        // gRPC DEMO - Oyuncu Bilgisi + AI Tahmini
        // ==========================================
        public IActionResult GrpcDemo()
        {
            ViewBag.GrpcEndpoint = "http://localhost:5001";

            // gRPC client eklemek için Grpc.Net.Client paketi gerekir
            // Bu demo'da sadece dokümantasyon gösteriyoruz
            // Gerçek gRPC entegrasyonu için:
            // 1. NuGet: Grpc.Net.Client
            // 2. Proto dosyasını projeye ekle
            // 3. PlayerServiceClient oluştur

            ViewBag.GrpcNote = "gRPC servisi port 5001'de çalışıyor. gRPC client entegrasyonu için Grpc.Net.Client paketi gerekir.";

            return View();
        }

        // ==========================================
        // AJAX ENDPOINT - Node.js'den Oyuncu Getir
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetPlayerFromNodeApi(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                var response = await client.GetAsync($"{NODE_API_URL}/api/players/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return Content(json, "application/json");
                }
                else
                {
                    return Json(new { status = "error", message = $"HTTP {response.StatusCode}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

        // ==========================================
        // AJAX ENDPOINT - Hava Durumu
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetWeather(string city)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                var response = await client.GetAsync($"{NODE_API_URL}/api/weather/{city}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return Content(json, "application/json");
                }
                else
                {
                    return Json(new { status = "error", message = "Şehir bulunamadı" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

        // ==========================================
        // AJAX ENDPOINT - Döviz Kuru
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetExchangeRate(string from, string to)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                var response = await client.GetAsync($"{NODE_API_URL}/api/exchange/{from}/{to}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return Content(json, "application/json");
                }
                else
                {
                    return Json(new { status = "error", message = "Döviz bulunamadı" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }
    }
}
