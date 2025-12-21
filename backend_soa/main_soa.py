# Dosya: main_soa.py (AKILLI HİBRİT)
import requests
from utils.scraper import FutbolScraper
from database.local_data import LocalDataManager
import time

AI_SERVICE = "http://127.0.0.1:5001/tahmin_et"
DB_SERVICE = "http://127.0.0.1:5002/kaydet"

def main():
    print("\n" + "="*60)
    print("🚀 FUTBOL SCOUT SİSTEMİ (AKILLI HİBRİT)")
    print("="*60 + "\n")
    
    local_db = LocalDataManager()
    
    isim = input("👉 Oyuncu İsmi: ")
    if not isim: return

    oyuncu_verisi = None
    gercek_deger = 0
    tahmin_deger = 0
    kaynak = ""
    ai_kullanildi = False

    # ---------------------------------------------------------
    # 1. AŞAMA: YEREL KONTROL
    # ---------------------------------------------------------
    print("\n📂 Yerel veritabanı taranıyor...")
    local_sonuc = local_db.oyuncu_getir(isim)
    
    if local_sonuc:
        # Oyuncu dosyada var!
        oyuncu_verisi = local_sonuc
        kaynak = "YEREL (CSV)"
        
        # Gerçek Değeri Al
        gercek_deger = local_sonuc.get('Piyasa_Degeri_Euro', 0)
        
        # EĞER DOSYADA ZATEN TAHMİN VARSA DİREKT AL
        # (CSV sütun ismine göre burayı düzenleyebilirsin: 'Tahmin', 'AI_Tahmin' vb.)
        if 'Tahmin' in local_sonuc and local_sonuc['Tahmin'] > 0:
             tahmin_deger = local_sonuc['Tahmin']
        elif 'Tahmin_Deger' in local_sonuc and local_sonuc['Tahmin_Deger'] > 0:
             tahmin_deger = local_sonuc['Tahmin_Deger']
        
    else:
        # ---------------------------------------------------------
        # 2. AŞAMA: İNTERNET (Scraper)
        # ---------------------------------------------------------
        print("❌ Yerel dosyada yok. İnternete bağlanılıyor...")
        kaynak = "İNTERNET (WEB)"
        bot = FutbolScraper(headless=True)
        try:
            oyuncu_verisi = bot.fbref_veri_getir(isim)
            if oyuncu_verisi:
                gercek_deger = bot.transfermarkt_deger_getir(isim)
            else:
                print("❌ Oyuncu bulunamadı.")
                bot.kapat()
                return
        finally:
            bot.kapat()

    # KART BİLGİLERİ
    print(f"\n📊 OYUNCU KARTI ({kaynak})")
    print("-" * 30)
    print(f"İsim:   {oyuncu_verisi.get('Oyuncu')}")
    print(f"Takım:  {oyuncu_verisi.get('Takim')}")
    print(f"Gol:    {oyuncu_verisi.get('Gol')}")
    print(f"Piyasa: {gercek_deger:,.0f} €")
    if tahmin_deger > 0:
        print(f"Tahmin: {tahmin_deger:,.0f} € (Dosyadan Okundu)")
    print("-" * 30)

    # ---------------------------------------------------------
    # 3. AŞAMA: AI TAHMİNİ (Gerekirse)
    # ---------------------------------------------------------
    
    # Eğer dosyada tahmin yoksa VEYA veri internetten geldiyse -> AI ÇALIŞSIN
    if tahmin_deger == 0:
        print(f"\n🧠 Yapay Zeka Hesaplanıyor...")
        try:
            resp = requests.post(AI_SERVICE, json=oyuncu_verisi)
            if resp.status_code == 200:
                tahmin_deger = resp.json()['tahmin']
                ai_kullanildi = True
                print(f"✅ Hesaplama Tamamlandı.")
            else:
                print("❌ AI Hatası:", resp.text)
        except Exception as e:
            print(f"❌ AI Servisine Bağlanılamadı: {e}")

    # SONUÇ EKRANI
    if tahmin_deger > 0:
        print("\n" + "*"*40)
        kaynak_text = "AI TAHMİNİ" if ai_kullanildi else "DOSYA KAYDI"
        print(f"🎯 {kaynak_text}: {tahmin_deger:,.0f} €")
        print("*"*40)
        
        if gercek_deger > 0:
            fark = tahmin_deger - gercek_deger
            durum = "UCUZ" if fark > 0 else "PAHALI"
            print(f"💡 Durum: Piyasa değerinden {abs(fark):,.0f} € daha {durum}.")

        # Sadece AI yeni hesap yaptıysa veritabanına kaydedelim
        if ai_kullanildi:
            requests.post(DB_SERVICE, json={
                'isim': oyuncu_verisi.get('Oyuncu'),
                'takim': oyuncu_verisi.get('Takim'),
                'yas': oyuncu_verisi.get('Yas'),
                'gercek_deger': gercek_deger,
                'tahmin_deger': tahmin_deger
            })
            print("\n💾 Yeni analiz veritabanına kaydedildi.")
    else:
        print("\n⚠️ Bir hata oluştu, tahmin üretilemedi.")

if __name__ == "__main__":
    main()