# Dosya: src/db_manager.py
import sqlite3
import os
from datetime import datetime

class DatabaseManager:
    def __init__(self, db_name='database/futbol.db'):
        # Klasör yoksa oluştur
        os.makedirs(os.path.dirname(db_name), exist_ok=True)
        
        self.conn = sqlite3.connect(db_name)
        self.cursor = self.conn.cursor()
        self.tablolari_olustur()
        
    def tablolari_olustur(self):
        """Eğer yoksa veritabanı tablolarını kurar."""
        query = """
        CREATE TABLE IF NOT EXISTS Oyuncular (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            isim TEXT,
            takim TEXT,
            yas INTEGER,
            gercek_deger REAL,
            tahmin_deger REAL,
            fark REAL,
            tarih DATETIME
        )
        """
        self.cursor.execute(query)
        self.conn.commit()
        
    def oyuncu_ekle(self, isim, takim, yas, gercek, tahmin):
        """Yeni bir analiz sonucunu kaydeder."""
        fark = tahmin - gercek
        tarih = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        
        query = """
        INSERT INTO Oyuncular (isim, takim, yas, gercek_deger, tahmin_deger, fark, tarih)
        VALUES (?, ?, ?, ?, ?, ?, ?)
        """
        self.cursor.execute(query, (isim, takim, yas, gercek, tahmin, fark, tarih))
        self.conn.commit()
        print(f"💾 Veritabanına kaydedildi: {isim}")

    def tum_oyunculari_getir(self):
        """Arayüzde göstermek için hepsini çeker."""
        self.cursor.execute("SELECT * FROM Oyuncular ORDER BY tarih DESC")
        return self.cursor.fetchall()

    def kapat(self):
        self.conn.close()