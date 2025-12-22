#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
PostgreSQL BCrypt Hash Duzeltme Script
Admin kullanicisinin hash'ini gunceller
"""

import psycopg2
from psycopg2 import Error
import sys

# UTF-8 encoding fix for Windows
if sys.platform == 'win32':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')
    sys.stderr = codecs.getwriter('utf-8')(sys.stderr.buffer, 'strict')

def main():
    # Baglanti bilgileri
    conn_params = {
        'host': 'localhost',
        'port': 5432,
        'database': 'ScoutDB',
        'user': 'postgres',
        'password': 'admin'
    }

    # Dogru BCrypt hash (sifre: 123456)
    correct_hash = "$2a$11$mOnXvnF4IG3vFs3.0pyQ7eHVNROThVLKXIN6L3orXVW4N.VaVpjYy"

    try:
        # Veritabanina baglan
        print("[*] Veritabanina baglaniyor...")
        conn = psycopg2.connect(**conn_params)
        cursor = conn.cursor()
        print("[OK] Baglanti basarili!")

        # 1. Mevcut kullanclar listele
        print("\n Mevcut kullanclar:")
        cursor.execute("SELECT user_id, username, LEFT(password_hash, 20) || '...', email, role_id FROM users")
        users = cursor.fetchall()
        for user in users:
            print(f"  ID: {user[0]}, User: {user[1]}, Hash: {user[2]}, Email: {user[3]}, Role: {user[4]}")

        # 2. Admin kullancsnn hash'ini kontrol et
        print("\n Admin kullancsnn mevcut hash'i:")
        cursor.execute("SELECT user_id, username, password_hash FROM users WHERE username = 'admin'")
        admin = cursor.fetchone()

        if admin:
            print(f"  ID: {admin[0]}, User: {admin[1]}")
            print(f"  Hash: {admin[2]}")

            if admin[2] == correct_hash:
                print("\n Admin hash'i zaten doru!")
            else:
                print("\n  Admin hash'i geersiz! Gncelleniyor...")

                # 3. Hash'i gncelle
                cursor.execute(
                    "UPDATE users SET password_hash = %s WHERE username = 'admin'",
                    (correct_hash,)
                )
                conn.commit()
                print(f" {cursor.rowcount} kullanc gncellendi!")

                # 4. Dorula
                cursor.execute("SELECT password_hash FROM users WHERE username = 'admin'")
                new_hash = cursor.fetchone()[0]
                print(f"\n Yeni hash: {new_hash[:30]}...")

                if new_hash == correct_hash:
                    print(" Hash doruland!")
                else:
                    print(" Hash gncellenemedi!")
        else:
            print(" Admin kullancs bulunamad!")
            print("\n Admin kullancs oluturuluyor...")
            cursor.execute("""
                INSERT INTO users (username, password_hash, email, role_id, created_at)
                VALUES ('admin', %s, 'admin@scout.com', 1, NOW())
            """, (correct_hash,))
            conn.commit()
            print(" Admin kullancs oluturuldu!")

        # 5. Dier kullanclar kontrol et
        print("\n Dier kullanclar kontrol ediliyor...")
        cursor.execute("SELECT user_id, username, password_hash FROM users WHERE username != 'admin'")
        other_users = cursor.fetchall()

        invalid_users = []
        for user in other_users:
            hash_val = user[2]
            if not (hash_val.startswith('$2a$') or hash_val.startswith('$2b$') or hash_val.startswith('$2y$')):
                invalid_users.append(user[1])

        if invalid_users:
            print(f"  {len(invalid_users)} kullancnn hash'i geersiz:")
            for username in invalid_users:
                print(f"   - {username}")
            print("\n Bu kullanclar iin 'ifremi Unuttum' zellii kullanlmal veya yeni ifre oluturulmal.")
        else:
            print(" Tm kullanclarn hash'leri geerli!")

        print("\n" + "="*50)
        print(" lem tamamland!")
        print("="*50)
        print("\n Test bilgileri:")
        print("  Kullanc ad: admin")
        print("  ifre: 123456")
        print("  URL: http://localhost:5199/Account/Login")

        cursor.close()
        conn.close()

    except Error as e:
        print(f"\n PostgreSQL Hatas: {e}")
    except Exception as e:
        print(f"\n Beklenmeyen Hata: {e}")

if __name__ == "__main__":
    main()
