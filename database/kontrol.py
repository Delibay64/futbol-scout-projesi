import pandas as pd

# Dosya adın neyse onu yaz
df = pd.read_csv('Final_Veriler_Kalecisiz.csv')

print("--- İŞTE SENİN DOSYADAKİ GERÇEK SÜTUN İSİMLERİ ---")
print(df.columns.tolist())
print("--------------------------------------------------")