@echo off
echo ========================================
echo Python ML Servisi Baslatiliyor...
echo ========================================
echo.

cd ml_service

echo Python ML Model Servisi
echo Port: 5000
echo Endpoint: http://localhost:5000/predict
echo.

python ai_service.py

pause
