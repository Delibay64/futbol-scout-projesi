import joblib
import pandas as pd
import numpy as np
import os

class ScoutAI:
    def __init__(self, model_path='models/futbol_zeka_sistemi.pkl'):
        """
        Yapay zeka modelini ve sözlükleri yükler.
        """
        # Model path kontrolü (main.py'den çağrılınca path ../ değil models/ olur)
        if not os.path.exists(model_path):
             # Belki bir üst klasördedir diye kontrol edelim
            if os.path.exists(f"../{model_path}"):
                model_path = f"../{model_path}"
            else:
                raise FileNotFoundError(f"❌ Model dosyası bulunamadı: {model_path}")
            
        print(f"🧠 Yapay Zeka Motoru Yükleniyor... ({model_path})")
        self.system = joblib.load(model_path)
        self.model = self.system['model']
        self.team_map = self.system['team_map']
        self.league_map = self.system['league_map']
        self.global_mean = self.system['global_mean']
        print("✅ Sistem Hazır!")

    def tahmin_et(self, oyuncu_verisi):
        """
        Dışarıdan gelen sözlük (dict) verisini alır, tahmin yapar.
        """
        # 1. Sözlüğü DataFrame'e çevir
        df = pd.DataFrame([oyuncu_verisi])
        
        # 2. Takım ve Lig Gücünü Eşleştir
        takim = df.iloc[0]['Takim']
        df['Takim_Gucu'] = self.team_map.get(takim, self.global_mean)
        
        lig = df.iloc[0]['Lig']
        df['Lig_Gucu'] = self.league_map.get(lig, self.global_mean)
        
        # 3. Modelin Tanımadığı Sütunları Temizle
        features = df.drop(columns=['Oyuncu', 'Takim', 'Lig'], errors='ignore')
        
        # 4. Tahmin Yap
        log_sonuc = self.model.predict(features)
        
        # 5. Gerçek Paraya Çevir
        gercek_fiyat = np.expm1(log_sonuc)[0]
        
        return int(gercek_fiyat)