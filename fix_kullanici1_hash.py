#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
kullanici1 BCrypt Hash Duzeltme Script
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

    # Dogru BCrypt hash'ler
    # kullanici1 icin yeni sifre: 123456
    kullanici1_hash = "$2a$11$mOnXvnF4IG3vFs3.0pyQ7eHVNROThVLKXIN6L3orXVW4N.VaVpjYy"

    try:
        print("[*] Veritabanina baglaniyor...")
        conn = psycopg2.connect(**conn_params)
        cursor = conn.cursor()
        print("[OK] Baglanti basarili!")

        # kullanici1'in hash'ini guncelle
        print("\n[*] kullanici1 hash'i guncelleniyor...")
        cursor.execute(
            "UPDATE users SET password_hash = %s WHERE username = %s",
            (kullanici1_hash, 'kullanıcı1')
        )
        conn.commit()
        print(f"[OK] {cursor.rowcount} kullanici guncellendi!")

        # Dogrula
        cursor.execute("SELECT user_id, username, password_hash FROM users WHERE username = %s", ('kullanıcı1',))
        user = cursor.fetchone()

        if user:
            print(f"\n[CHECK] Kullanici ID: {user[0]}, Username: {user[1]}")
            print(f"[CHECK] Hash: {user[2][:30]}...")

            if user[2] == kullanici1_hash:
                print("\n[OK] Hash dogruland! Giris bilgileri:")
                print("  Kullanici adi: kullanici1")
                print("  Sifre: 123456")
            else:
                print("\n[ERROR] Hash guncellenemedi!")
        else:
            print("\n[ERROR] kullanici1 bulunamadi!")

        cursor.close()
        conn.close()

    except Error as e:
        print(f"\n[ERROR] PostgreSQL Hatasi: {e}")
    except Exception as e:
        print(f"\n[ERROR] Beklenmeyen Hata: {e}")

if __name__ == "__main__":
    main()
