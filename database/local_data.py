# Dosya: src/local_data.py
import pandas as pd
import numpy as np
import os

class LocalDataManager:
    def __init__(self, csv_path='Final_Veriler_Kalecisiz.csv'):
        # Dosya yolunu bul
        if not os.path.exists(csv_path):
            if os.path.exists(f"../{csv_path}"):
                csv_path = f"../{csv_path}"
        
        try:
            self.df = pd.read_csv(csv_path)
            # Arama kolaylığı için küçük harf sütunu
            self.df['Oyuncu_Lower'] = self.df['Oyuncu'].astype(str).str.lower()
            print(f"📂 Yerel Veri Yüklendi: {len(self.df)} Oyuncu hazır.")
        except Exception as e:
            print(f"⚠️ CSV Okuma Hatası: {e}")
            self.df = pd.DataFrame()

    def oyuncu_getir(self, isim):
        if self.df.empty: return None
        
        isim_kucuk = isim.lower().strip()
        
        # 1. Tam Eşleşme
        bulunan = self.df[self.df['Oyuncu_Lower'] == isim_kucuk]
        
        # 2. İçeren Eşleşme (Lamine Yamal yazınca bulsun)
        if bulunan.empty:
            bulunan = self.df[self.df['Oyuncu_Lower'].str.contains(isim_kucuk, na=False)]
        
        if not bulunan.empty:
            # En iyi eşleşmeyi al
            row = bulunan.iloc[0]
            print(f"✅ Yerel Dosyada Bulundu: {row['Oyuncu']}")
            
            # PANDAS -> PYTHON SÖZLÜK (Temizleme İşlemi)
            # Numpy tiplerini (int64) standart Python int/float'a çevirmeliyiz
            data = {}
            for col in row.index:
                val = row[col]
                if isinstance(val, (np.integer, np.int64)):
                    val = int(val)
                elif isinstance(val, (np.floating, np.float64)):
                    val = float(val)
                data[col] = val
            
            # Gereksiz yardımcı sütunu sil
            if 'Oyuncu_Lower' in data: del data['Oyuncu_Lower']
            
            return data
        else:
            return None