import pandas as pd
import numpy as np
import joblib
import os
from sklearn.ensemble import GradientBoostingRegressor

def clean_train_save():
    print("🧹 Model Temizliği ve Yeniden Eğitim Başlıyor...")

    # 1. VERİYİ OKU
    if not os.path.exists("Final_Veriler_Kalecisiz.csv"):
        print("❌ HATA: CSV dosyası yok!")
        return
        
    df = pd.read_csv("Final_Veriler_Kalecisiz.csv")
    
    # Gereksizleri at
    if 'Yas_Detay' in df.columns: 
        df.drop(columns=['Yas_Detay'], inplace=True)
    
    # Filtre (75M altı)
    df = df[df['Piyasa_Degeri_Euro'] <= 75000000]

    # 2. FEATURE ENGINEERING (Özellik Üretimi)
    # Log Transform
    df['Log_Fiyat'] = np.log1p(df['Piyasa_Degeri_Euro'])

    # --- SÖZLÜKLERİ OLUŞTUR ---
    # Takım ve Lig güçlerini hesapla ve sakla
    global_mean = df['Piyasa_Degeri_Euro'].mean()
    
    team_map = df.groupby('Takim')['Piyasa_Degeri_Euro'].mean().to_dict()
    league_map = df.groupby('Lig')['Piyasa_Degeri_Euro'].mean().to_dict()

    # Dataframe'e uygula
    df['Takim_Gucu'] = df['Takim'].map(team_map)
    df['Lig_Gucu'] = df['Lig'].map(league_map)

    # Diğer kategorik verileri (Ayak, Mevki vb.) kodla
    # AMA Takim ve Lig'i hariç tutacağız çünkü onları drop edeceğiz.
    cat_cols = df.select_dtypes(include=['object']).columns
    cat_maps = {} # İleride lazım olabilir diye saklayalım
    
    for col in cat_cols:
        if col not in ['Oyuncu', 'Takim', 'Lig']: # Takim ve Lig'e dokunma, sileceğiz
            df[col] = df[col].astype('category')
            cat_maps[col] = dict(enumerate(df[col].cat.categories))
            df[col] = df[col].cat.codes

    # 3. EĞİTİM SETİNİ HAZIRLA (KRİTİK KISIM BURASI 🚨)
    # Modelin kafasını karıştıran 'Takim' ve 'Lig' sütunlarını burada SİLİYORUZ.
    # Sadece 'Takim_Gucu' kalıyor.
    X = df.drop(columns=['Oyuncu', 'Piyasa_Degeri_Euro', 'Log_Fiyat', 'Takim', 'Lig'])
    y = df['Log_Fiyat']
    
    print(f"ℹ️ Eğitimde kullanılan sütun sayısı: {X.shape[1]}")
    # print(f"Sütunlar: {list(X.columns)}") # Merak edersen aç bak

    # 4. MODELİ EĞİT
    print("🤖 Model eğitiliyor (Gradient Boosting)...")
    model = GradientBoostingRegressor(n_estimators=500, learning_rate=0.05, max_depth=4, random_state=42)
    model.fit(X, y)

    # 5. SİSTEMİ PAKETLE VE KAYDET
    system_files = {
        'model': model,
        'team_map': team_map,
        'league_map': league_map,
        'global_mean': global_mean,
        'cat_maps': cat_maps,
        'training_columns': list(X.columns) # Hangi sütunlarla eğitildiğini bilelim
    }

    # Klasör yoksa oluştur
    os.makedirs('models', exist_ok=True)
    
    joblib.dump(system_files, 'models/futbol_zeka_sistemi.pkl')
    print("✅ MÜKEMMEL! Yeni ve temiz model kaydedildi: models/futbol_zeka_sistemi.pkl")
    print("👉 Şimdi 'python main.py' çalıştırabilirsin.")

if __name__ == "__main__":
    clean_train_save()