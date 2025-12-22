using System;
using Npgsql;

class Program
{
    static void Main(string[] args)
    {
        var connString = "Host=localhost;Port=5432;Database=ScoutDB;Username=postgres;Password=admin";

        try
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                Console.WriteLine("✅ Veritabanına bağlandı!");

                // 1. Mevcut kullanıcıları listele
                Console.WriteLine("\n📋 Mevcut kullanıcılar:");
                using (var cmd = new NpgsqlCommand("SELECT user_id, username, password_hash, email, role_id FROM users", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"  ID: {reader.GetInt32(0)}, User: {reader.GetString(1)}, Hash: {reader.GetString(2).Substring(0, Math.Min(15, reader.GetString(2).Length))}..., Email: {reader.GetString(3)}");
                    }
                }

                // 2. Admin kullanıcısının hash'ini güncelle
                Console.WriteLine("\n🔧 Admin kullanıcısının hash'i güncelleniyor...");
                var correctHash = "$2a$11$mOnXvnF4IG3vFs3.0pyQ7eHVNROThVLKXIN6L3orXVW4N.VaVpjYy";

                using (var cmd = new NpgsqlCommand("UPDATE users SET password_hash = @hash WHERE username = 'admin'", conn))
                {
                    cmd.Parameters.AddWithValue("hash", correctHash);
                    var affectedRows = cmd.ExecuteNonQuery();
                    Console.WriteLine($"✅ {affectedRows} kullanıcı güncellendi!");
                }

                // 3. Diğer kullanıcıları kontrol et ve gerekirse düzelt
                Console.WriteLine("\n🔍 Diğer kullanıcılar kontrol ediliyor...");
                using (var cmd = new NpgsqlCommand("SELECT user_id, username, password_hash FROM users WHERE username != 'admin'", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    var usersToFix = new System.Collections.Generic.List<(int id, string username, string hash)>();
                    while (reader.Read())
                    {
                        var hash = reader.GetString(2);
                        if (!hash.StartsWith("$2a$") && !hash.StartsWith("$2b$") && !hash.StartsWith("$2y$"))
                        {
                            usersToFix.Add((reader.GetInt32(0), reader.GetString(1), hash));
                        }
                    }

                    if (usersToFix.Count > 0)
                    {
                        Console.WriteLine($"⚠️  {usersToFix.Count} kullanıcının hash'i geçersiz format:");
                        foreach (var user in usersToFix)
                        {
                            Console.WriteLine($"   - {user.username} (ID: {user.id})");
                        }
                    }
                    else
                    {
                        Console.WriteLine("✅ Tüm kullanıcıların hash'leri geçerli!");
                    }
                }

                // 4. Doğrulama
                Console.WriteLine("\n✅ Güncelleme tamamlandı!");
                Console.WriteLine("\n📝 Test için:");
                Console.WriteLine("  Kullanıcı adı: admin");
                Console.WriteLine("  Şifre: 123456");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ HATA: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Detay: {ex.InnerException.Message}");
            }
        }
    }
}
