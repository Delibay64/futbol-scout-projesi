# Dosya: src/db_service.py
from flask import Flask, request, jsonify
import sqlite3
import os
from datetime import datetime

app = Flask(__name__)
DB_PATH = 'database/futbol.db'

# Klasör yoksa oluştur
os.makedirs('database', exist_ok=True)

def init_db():
    with sqlite3.connect(DB_PATH) as conn:
        cursor = conn.cursor()
        cursor.execute("""
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
        """)
        conn.commit()

init_db() # Başlarken tabloyu kur

@app.route('/kaydet', methods=['POST'])
def kaydet():
    try:
        data = request.json
        
        isim = data.get('isim')
        takim = data.get('takim')
        yas = data.get('yas')
        gercek = data.get('gercek_deger')
        tahmin = data.get('tahmin_deger')
        fark = tahmin - gercek
        tarih = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

        with sqlite3.connect(DB_PATH) as conn:
            cursor = conn.cursor()
            cursor.execute("""
            INSERT INTO Oyuncular (isim, takim, yas, gercek_deger, tahmin_deger, fark, tarih)
            VALUES (?, ?, ?, ?, ?, ?, ?)
            """, (isim, takim, yas, gercek, tahmin, fark, tarih))
            conn.commit()
        
        print(f"💾 DB Servisi: {isim} kaydedildi.")
        return jsonify({'status': 'success'})

    except Exception as e:
        return jsonify({'status': 'error', 'mesaj': str(e)}), 500

if __name__ == '__main__':
    # 5002 Portundan yayın yap
    app.run(port=5002, debug=True)