@echo off
echo ========================================
echo Node.js API Servisi Kurulum
echo ========================================
echo.

cd nodejs_api

echo [1/2] Node.js versiyonu kontrol ediliyor...
node --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [HATA] Node.js yuklu degil!
    echo Lutfen https://nodejs.org adresinden yukleyin
    pause
    exit /b 1
)

echo [OK] Node.js:
node --version

echo.
echo [2/2] NPM paketleri yukleniyor...
echo - express (5.2.1)
echo - cors (2.8.5)
echo - pg (8.16.3)
echo - soap (1.6.1)
echo - node-fetch (3.3.2)
echo.

npm install

if %errorlevel% equ 0 (
    echo.
    echo [OK] Tum paketler yuklendi
) else (
    echo [HATA] Paket yukleme hatasi!
    pause
    exit /b 1
)

echo.
echo ========================================
echo VERITABANI BAGLANTISI
echo ========================================
echo.
echo server.js dosyasindaki PostgreSQL sifresini kontrol edin!
echo.
echo const pool = new Pool({
echo   user: 'postgres',
echo   host: 'localhost',
echo   database: 'scoutdb',
echo   password: '1234',  ^<-- Kendi sifreniz
echo   port: 5432
echo });
echo.

cd ..

echo ========================================
echo NODE.JS KURULUMU TAMAMLANDI!
echo ========================================
echo.
echo Servisi baslatmak icin:
echo cd nodejs_api
echo node server.js
echo.
echo veya start_all_windows.bat calistirin
echo ========================================
pause
