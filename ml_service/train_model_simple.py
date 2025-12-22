# -*- coding: utf-8 -*-
import pandas as pd
import numpy as np
import joblib
import os

print("Model egitimi basladi...")

# CSV oku
df = pd.read_csv("Final_Veriler_Kalecisiz.csv")

if 'Yas_Detay' in df.columns:
    df.drop(columns=['Yas_Detay'], inplace=True)

df = df[df['Piyasa_Degeri_Euro'] <= 75000000]

# Log transform
df['Log_Fiyat'] = np.log1p(df['Piyasa_Degeri_Euro'])

# Sozlukler
global_mean = df['Piyasa_Degeri_Euro'].mean()
team_map = df.groupby('Takim')['Piyasa_Degeri_Euro'].mean().to_dict()
league_map = df.groupby('Lig')['Piyasa_Degeri_Euro'].mean().to_dict()

df['Takim_Gucu'] = df['Takim'].map(team_map)
df['Lig_Gucu'] = df['Lig'].map(league_map)

# Kategorik kodlama
cat_cols = df.select_dtypes(include=['object']).columns
for col in cat_cols:
    if col not in ['Oyuncu', 'Takim', 'Lig']:
        df[col] = df[col].astype('category').cat.codes

# X ve y hazirla
X = df.drop(columns=['Oyuncu', 'Piyasa_Degeri_Euro', 'Log_Fiyat', 'Takim', 'Lig'])
y = df['Log_Fiyat']

print(f"Sutun sayisi: {X.shape[1]}")

# Model egit
from sklearn.ensemble import GradientBoostingRegressor
model = GradientBoostingRegressor(n_estimators=500, learning_rate=0.05, max_depth=4, random_state=42)
model.fit(X, y)

# Kaydet
system = {
    'model': model,
    'team_map': team_map,
    'league_map': league_map,
    'global_mean': global_mean,
    'training_columns': list(X.columns)
}

os.makedirs('models', exist_ok=True)
joblib.dump(system, 'models/futbol_zeka_sistemi.pkl')
print("Model kaydedildi: models/futbol_zeka_sistemi.pkl")
