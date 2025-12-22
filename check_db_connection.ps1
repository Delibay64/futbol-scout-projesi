# PostgreSQL bağlantı testi ve kullanıcı hash kontrolü
# PowerShell script

$env:PGPASSWORD = "admin"
$psqlPath = "C:\Program Files\PostgreSQL\16\bin\psql.exe"

# psql yolunu bul
if (-not (Test-Path $psqlPath)) {
    $psqlPath = "C:\Program Files\PostgreSQL\15\bin\psql.exe"
}
if (-not (Test-Path $psqlPath)) {
    $psqlPath = "C:\Program Files\PostgreSQL\14\bin\psql.exe"
}

if (-not (Test-Path $psqlPath)) {
    Write-Host "❌ HATA: psql bulunamadı!" -ForegroundColor Red
    Write-Host "PostgreSQL kurulu mu kontrol edin." -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ psql bulundu: $psqlPath" -ForegroundColor Green

# Kullanıcıları listele
Write-Host "`n📋 Veritabanındaki kullanıcılar:" -ForegroundColor Cyan
$query1 = "SELECT user_id, username, LEFT(password_hash, 10) || '...' as hash_preview, email, role_id FROM users;"
& $psqlPath -h localhost -p 5432 -U postgres -d ScoutDB -c $query1

# Admin kullanıcısının hash'ini kontrol et
Write-Host "`n🔍 Admin kullanıcısının tam hash'i:" -ForegroundColor Cyan
$query2 = "SELECT user_id, username, password_hash, email FROM users WHERE username = 'admin';"
& $psqlPath -h localhost -p 5432 -U postgres -d ScoutDB -c $query2

Write-Host "`n✅ Kontrol tamamlandı!" -ForegroundColor Green
