using System;

// "admin" şifresinin BCrypt hash'ini üret
var hash = BCrypt.Net.BCrypt.HashPassword("admin");
Console.WriteLine($"BCrypt Hash for 'admin': {hash}");

// Test et
var isValid = BCrypt.Net.BCrypt.Verify("admin", hash);
Console.WriteLine($"Verification test: {isValid}");