@echo off
echo ================================================
echo    PYTHON FLASK AI SERVISI BASLATILIYOR
echo ================================================
echo.
echo Bu servis ML tahmin ve veri cekme icin gereklidir.
echo.
echo Port: 5000
echo Endpoints: /predict, /scrape_player
echo.
echo ================================================
echo.

cd /d "%~dp0ml_service"

echo [*] Gerekli Python kutuphaneleri kontrol ediliyor...
python -c "import flask, joblib, pandas, numpy, requests, bs4" 2>nul
if errorlevel 1 (
    echo.
    echo [HATA] Gerekli kutuphaneler yuklenmemis!
    echo.
    echo Asagidaki komutu calistirin:
    echo pip install flask joblib pandas numpy scikit-learn requests beautifulsoup4
    echo.
    pause
    exit /b 1
)

echo [OK] Tum kutuphaneler mevcut!
echo.

echo [*] Model dosyasi kontrol ediliyor...
if not exist "futbol_zeka_sistemi.pkl" (
    echo.
    echo [UYARI] Model dosyasi bulunamadi: futbol_zeka_sistemi.pkl
    echo AI tahmin ozelligi calismayacak.
    echo.
)

echo.
echo ================================================
echo [*] Flask servisi baslatiliyor...
echo ================================================
echo.
echo Durdurmak icin: Ctrl+C
echo.

python ai_service.py

pause
