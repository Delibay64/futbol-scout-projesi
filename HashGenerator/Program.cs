// Doğru BCrypt hash oluştur
var hash = BCrypt.Net.BCrypt.HashPassword("admin");
Console.WriteLine($"BCrypt Hash: {hash}");

// Doğrulama testi
var isValid = BCrypt.Net.BCrypt.Verify("admin", hash);
Console.WriteLine($"Test: {isValid}");