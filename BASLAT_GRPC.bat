@echo off
echo ========================================
echo gRPC Servisi Baslatiliyor...
echo ========================================
echo.

cd web_ui\ScoutGrpcService

echo gRPC Player Service + ML Entegrasyonu
echo Port: 5001 (HTTP/2)
echo.

dotnet run --urls "http://localhost:5001"

pause
