@echo off
echo ============================================
echo   Node.js API ve SOAP Servisi Baslat
echo ============================================
echo.

cd nodejs_api

echo PostgreSQL baglanti bilgileri:
echo   Host: localhost
echo   Database: ScoutDB
echo   User: postgres
echo   Password: admin
echo.

echo Node.js servisi baslatiliyor...
echo Port: 3000
echo.

node server.js

pause
