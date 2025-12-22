@echo off
REM ========================================
REM FUTBOL SCOUT - VERİTABANI GÜNCELLEMESİ
REM ========================================

echo.
echo ========================================
echo FUTBOL SCOUT - VERİTABANI GÜNCELLEMESİ
echo ========================================
echo.
echo Bu script mevcut ScoutDB veritabanınıza:
echo   - 2 yeni Stored Procedure
echo   - 11 performans Index
echo   - 8 CHECK Constraint
echo ekleyecektir.
echo.

set /p CONFIRM="Devam etmek istiyor musunuz? (E/H): "
if /i not "%CONFIRM%"=="E" (
    echo İşlem iptal edildi.
    pause
    exit /b
)

echo.
echo Güncellemeler uygulanıyor...
echo.

REM PostgreSQL path'ini bul
set PSQL_PATH="C:\Program Files\PostgreSQL\18\bin\psql.exe"

if not exist %PSQL_PATH% (
    echo HATA: PostgreSQL bulunamadı!
    echo Lütfen PostgreSQL kurulumunuzu kontrol edin.
    pause
    exit /b 1
)

REM Migration scriptini çalıştır
%PSQL_PATH% -U postgres -d ScoutDB -f update_database.sql

echo.
if %errorlevel% equ 0 (
    echo ========================================
    echo GÜNCELLEME BAŞARILI!
    echo ========================================
    echo.
    echo Artık veritabanınızda:
    echo   ✓ 3 Stored Procedure
    echo   ✓ 11 Performans Index
    echo   ✓ 8 CHECK Constraint
    echo bulunmaktadır.
    echo.
    echo Uygulamanızı test edebilirsiniz:
    echo   cd ..\web_ui\ScoutWeb
    echo   dotnet run
) else (
    echo ========================================
    echo HATA OLUŞTU!
    echo ========================================
    echo.
    echo Lütfen hata mesajlarını kontrol edin.
    echo PostgreSQL şifrenizi doğru girdiğinizden emin olun.
)

echo.
pause
