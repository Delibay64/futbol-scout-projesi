@echo off
echo ========================================
echo Futbol Scout Projesi - Tum Servisler
echo ========================================
echo.

echo [1/4] PostgreSQL kontrol ediliyor...
pg_isready -h localhost -p 5432 >nul 2>&1
if %errorlevel% neq 0 (
    echo [HATA] PostgreSQL calisimiyor! Lutfen PostgreSQL servisini baslatin.
    pause
    exit /b 1
)
echo [OK] PostgreSQL hazir

echo.
echo [2/4] Python ML Servisi baslatiliyor (Port 5000)...
start "Python ML Service" cmd /k "cd ml_service && python simple_service.py"
timeout /t 3 >nul

echo.
echo [3/4] Node.js API Servisi baslatiliyor (Port 3000)...
start "Node.js API Service" cmd /k "cd nodejs_api && node server.js"
timeout /t 3 >nul

echo.
echo [4/4] ASP.NET Web UI baslatiliyor (Port 5199/7139)...
start "ASP.NET Web UI" cmd /k "cd web_ui\ScoutWeb && dotnet run"
timeout /t 3 >nul

echo.
echo ========================================
echo TUM SERVISLER BASLATILDI!
echo ========================================
echo.
echo Servisler:
echo - Python ML: http://localhost:5000
echo - Node.js API: http://localhost:3000
echo - Web UI: http://localhost:5199
echo.
echo Web tarayicinizda acmak icin:
echo http://localhost:5199
echo.
echo Cikis icin tum terminal penceresini kapatin.
echo ========================================
pause
