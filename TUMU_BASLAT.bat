@echo off
echo ========================================================
echo        TUM SOA SERVİSLERİNİ BASLATMA
echo ========================================================
echo.
echo Bu script 4 terminal açacak:
echo   1. Node.js API (REST + SOAP) - Port 3000
echo   2. Python ML Servisi - Port 5000
echo   3. gRPC Servisi - Port 5001
echo   4. ASP.NET Core Web - Port 5199
echo.
pause
echo.

echo [1/4] Node.js API baslatiliyor...
start cmd /k "cd nodejs_api && node server.js"
timeout /t 3

echo [2/4] Python ML Servisi baslatiliyor...
start cmd /k "cd ml_service && python ai_service.py"
timeout /t 3

echo [3/4] gRPC Servisi baslatiliyor...
start cmd /k "cd web_ui\ScoutGrpcService && dotnet run"
timeout /t 5

echo [4/4] Web Sitesi baslatiliyor...
start cmd /k "cd web_ui\ScoutWeb && dotnet run"

echo.
echo ========================================================
echo   TUM SERVİSLER BASLATILDI!
echo ========================================================
echo.
echo Tarayiciyi acmak icin bir tusa basin...
pause

start http://localhost:5199
