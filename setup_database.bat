@echo off
echo ========================================
echo Veritabani Kurulum Script'i
echo ========================================
echo.

set /p PGPASSWORD="PostgreSQL sifresi (default: 1234): "
if "%PGPASSWORD%"=="" set PGPASSWORD=1234

echo.
echo [1/5] Veritabani olusturuluyor...
psql -h localhost -U postgres -c "CREATE DATABASE scoutdb;" 2>nul
if %errorlevel% equ 0 (
    echo [OK] scoutdb veritabani olusturuldu
) else (
    echo [INFO] scoutdb zaten mevcut (bu normaldir)
)

echo.
echo [2/5] Ana sema yukleniyor...
cd database
psql -h localhost -U postgres -d scoutdb -f create_scoutdb.sql
if %errorlevel% equ 0 (
    echo [OK] Ana sema yuklendi
) else (
    echo [HATA] Ana sema yuklenemedi!
    pause
    exit /b 1
)

echo.
echo [3/5] Admin kullanicisi ekleniyor...
psql -h localhost -U postgres -d scoutdb -f insert_admin.sql
echo [OK] Admin kullanicisi (admin/123456)

echo.
echo [4/5] Scout report onay sistemi yukleniyor...
psql -h localhost -U postgres -d scoutdb -f add_scoutreport_approval.sql
if %errorlevel% equ 0 (
    echo [OK] Onay sistemi yuklendi
)

echo.
echo [5/5] CASCADE delete ekleniyor...
psql -h localhost -U postgres -d scoutdb -f add_cascade_delete_player_price_log.sql
if %errorlevel% equ 0 (
    echo [OK] CASCADE delete eklendi
)

cd ..

echo.
echo ========================================
echo VERITABANI KURULUMU TAMAMLANDI!
echo ========================================
echo.
echo Veritabani: scoutdb
echo Kullanici: admin
echo Sifre: 123456
echo.
echo Giris URL: http://localhost:5199
echo ========================================
pause
