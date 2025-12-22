@echo off
echo ========================================
echo Python ML Servisi Kurulum
echo ========================================
echo.

cd ml_service

echo [1/3] Sanal ortam olusturuluyor...
python -m venv venv
if %errorlevel% equ 0 (
    echo [OK] Sanal ortam olusturuldu
) else (
    echo [HATA] Sanal ortam olusturulamadi!
    pause
    exit /b 1
)

echo.
echo [2/3] Sanal ortam aktive ediliyor...
call venv\Scripts\activate.bat

echo.
echo [3/3] Gerekli paketler yukleniyor...
echo - Flask
echo - Joblib
echo - Pandas
echo - NumPy
echo - Scikit-learn
echo - Requests
echo - BeautifulSoup4
echo.

pip install flask joblib pandas numpy scikit-learn requests beautifulsoup4

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
echo ML MODEL EGITIMI
echo ========================================
echo.
echo [!] Final_Veriler_Kalecisiz.csv dosyasi gerekli!
echo.
set /p TRAIN="Modeli simdi egitmek ister misiniz? (E/H): "

if /i "%TRAIN%"=="E" (
    echo.
    echo Model egitiliyor...
    python train_model_simple.py

    if %errorlevel% equ 0 (
        echo [OK] Model basariyla egitildi!
        echo [OK] Dosya: models/futbol_zeka_sistemi.pkl
    ) else (
        echo [HATA] Model egitimi basarisiz!
    )
) else (
    echo [INFO] Model egitimi atland. Daha sonra egitmek icin:
    echo python train_model_simple.py
)

cd ..

echo.
echo ========================================
echo PYTHON KURULUMU TAMAMLANDI!
echo ========================================
echo.
echo Servisi baslatmak icin:
echo cd ml_service
echo python simple_service.py
echo.
echo veya start_all_windows.bat calistirin
echo ========================================
pause
