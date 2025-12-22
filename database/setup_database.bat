@echo off
REM ========================================
REM FUTBOL SCOUT - VERİTABANI KURULUM
REM ========================================

echo.
echo ========================================
echo FUTBOL SCOUT - POSTGRESQL KURULUMU
echo ========================================
echo.

REM Kullanıcıdan bilgileri al
set /p PG_PASSWORD="PostgreSQL 'postgres' kullanıcı şifrenizi girin: "

echo.
echo 1. ScoutDB veritabanı oluşturuluyor...
psql -U postgres -c "CREATE DATABASE ScoutDB;" 2>nul
if %errorlevel% equ 0 (
    echo    ✓ ScoutDB oluşturuldu
) else (
    echo    ! ScoutDB zaten mevcut veya hata oluştu
)

echo.
echo 2. Tablolar, fonksiyonlar ve view'lar oluşturuluyor...
psql -U postgres -d ScoutDB -f create_scoutdb.sql

echo.
echo ========================================
echo KURULUM TAMAMLANDI!
echo ========================================
echo.
echo Şimdi appsettings.json dosyasındaki şifreyi güncelleyin:
echo   "Password=%PG_PASSWORD%"
echo.
echo Sonra uygulamayı çalıştırabilirsiniz:
echo   cd ..\web_ui\ScoutWeb
echo   dotnet run
echo.

pause
