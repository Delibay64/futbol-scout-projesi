import sys
import os

# Bu dosyanın (main.py) olduğu yerin bir üst klasörünü (Proje Ana Dizinini) bulur
current_dir = os.path.dirname(os.path.abspath(__file__))
parent_dir = os.path.dirname(current_dir)

# Ana dizini Python'ın arama yoluna ekler
sys.path.append(parent_dir)

from ml_service.ai_engine import ScoutAI
from database.db_manager import DatabaseManager
import pandas as pd

def main():
    print("🚀 FUTBOL SCOUT SİSTEMİ BAŞLATILIYOR...")
    
    # 1. Motorları Çalıştır
    ai = ScoutAI()
    db = DatabaseManager()
    
    # 2. Modelin Eğitildiği Sütunları Bul (Eksik veri göndermemek için)
    gerekli_sutunlar = ai.model.feature_names_in_
    print(f"\nℹ️ Model şu sütunları bekliyor: {len(gerekli_sutunlar)} adet")
    
    # 3. ÖRNEK BİR OYUNCU SENARYOSU (Manuel Giriş)
    print("\n--- SANAL OYUNCU ANALİZİ ---")
    
    # Normalde burası Selenium'dan gelecek. 
    # Şimdilik model patlamasın diye 'boş' bir veri oluşturup sadece önemlileri dolduruyoruz.
    sanal_veri = {col: 0 for col in gerekli_sutunlar} # Hepsini 0 yap
    
    # Gerçek verileri üstüne yaz
    sanal_veri.update({
        'Takim': 'Besiktas JK',  # Modeldeki isimle AYNI olmalı
        'Lig': 'Super Lig',
        'Yas': 19,
        'Gol': 12,
        'Asist': 5,
        'Oynadigi_Sure_Dk': 1800,
        'Ilk_11': 20,
        # ... Diğer önemli istatistikler
    })
    
    # Manuel olarak ana kodda String olanlar (Takim, Lig) hariç tutulmalıydı ama
    # ai_engine içinde bunu drop ediyoruz zaten.
    # Ancak feature listesinde 'Takim_Gucu' ve 'Lig_Gucu' var, 'Takim' stringi yok.
    # Bu yüzden feature map'lemeyi doğru yapmamız lazım.
    
    # --- KRİTİK DÜZELTME ---
    # Model eğitilirken 'Takim' sütunu YOKTU, 'Takim_Gucu' VARDI.
    # ai_engine içinde bu dönüşümü yapıyoruz.
    # Ama model.feature_names_in_ listesinde 'Takim' string'i olmayacak.
    # Bu yüzden sözlüğe 'Takim' ve 'Lig' ekleyip gönderiyoruz, ai_engine hallediyor.
    
    oyuncu_ismi = "Sanal Semih"
    gercek_piyasa_degeri = 12000000 # 12M Euro (Örnek)
    
    # 4. TAHMİN ET
    try:
        tahmin = ai.tahmin_et(sanal_veri)
        print(f"\n🎯 {oyuncu_ismi} için Tahmin: {tahmin:,} €")
        print(f"💰 Gerçek Değer: {gercek_piyasa_degeri:,} €")
        
        # 5. VERİTABANINA KAYDET
        db.oyuncu_ekle(oyuncu_ismi, sanal_veri['Takim'], sanal_veri['Yas'], gercek_piyasa_degeri, tahmin)
        
    except Exception as e:
        print(f"❌ Hata oluştu: {e}")
        # Hata ayıklama için sütun farklarını göster
        print("Bunu düzeltmek için Selenium aşamasında tüm sütunları birebir eşleştireceğiz.")

if __name__ == "__main__":
    main()