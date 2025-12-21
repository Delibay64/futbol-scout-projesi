# Dosya: src/scraper.py (ZIRHLI VERSİYON)
from selenium import webdriver
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.common.by import By
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from webdriver_manager.chrome import ChromeDriverManager
import time
import re

class FutbolScraper:
    def __init__(self, headless=True):
        options = webdriver.ChromeOptions()
        if headless:
            options.add_argument("--headless=new") # Daha stabil headless modu
        
        # Bot yakalanmasını engelleyen ayarlar
        options.add_argument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36")
        options.add_argument("--start-maximized")
        options.add_argument("--disable-blink-features=AutomationControlled")
        options.add_argument("--disable-popup-blocking")
        options.add_argument("--lang=tr")
        options.add_argument("--log-level=3") # Gereksiz konsol yazılarını gizle
        
        self.driver = webdriver.Chrome(service=Service(ChromeDriverManager().install()), options=options)
        self.wait = WebDriverWait(self.driver, 15) # Bekleme süresini artırdım

    def fbref_veri_getir(self, oyuncu_ismi):
        print(f"🕵️‍♂️ FBref: '{oyuncu_ismi}' istatistikleri çekiliyor...")
        scraped_data = {}
        try:
            self.driver.get("https://fbref.com/en/")
            
            # Arama kutusunu bekle ve yaz
            try:
                search_box = self.wait.until(EC.presence_of_element_located((By.NAME, "search")))
                search_box.clear()
                search_box.send_keys(oyuncu_ismi)
                search_box.send_keys(Keys.RETURN)
            except:
                return None
            
            time.sleep(3)
            if "search" in self.driver.current_url:
                try: 
                    # İlk sonuca tıkla
                    link = self.driver.find_element(By.CSS_SELECTOR, ".search-item-name a")
                    link.click()
                except: return None
            
            # Tabloyu Bul
            try:
                tablo = self.wait.until(EC.presence_of_element_located((By.XPATH, "//table[contains(@id, 'stats_standard_')]")))
                son_satir = tablo.find_element(By.XPATH, ".//tbody/tr[last()]")
                
                scraped_data['Oyuncu'] = oyuncu_ismi
                scraped_data['Goals'] = self._get_text(son_satir, "goals")
                scraped_data['Assists'] = self._get_text(son_satir, "assists")
                scraped_data['Matches'] = self._get_text(son_satir, "mp")
                scraped_data['Minutes'] = self._get_text(son_satir, "min_playing_time", is_time=True)
                scraped_data['Age'] = self._get_text(son_satir, "age", is_age=True)
                try: scraped_data['Takim'] = son_satir.find_element(By.XPATH, ".//td[@data-stat='team']").text
                except: scraped_data['Takim'] = "Bilinmiyor"
                try: scraped_data['Lig'] = son_satir.find_element(By.XPATH, ".//td[@data-stat='comp_level']").text
                except: scraped_data['Lig'] = "Bilinmiyor"
            except: pass

            final_veri = {
                'Oyuncu': scraped_data.get('Oyuncu'),
                'Takim': scraped_data.get('Takim', 'Bilinmiyor'),
                'Lig': scraped_data.get('Lig', 'Bilinmiyor'),
                'Yas': scraped_data.get('Age', 25),
                'Gol': scraped_data.get('Goals', 0),
                'Asist': scraped_data.get('Assists', 0),
                'Mac_Sayisi': scraped_data.get('Matches', 0),
                'Oynadigi_Sure_Dk': scraped_data.get('Minutes', 0),
            }
            # Eksikleri 0 doldur
            for col in ['Ilk_11', 'Sut_Pas_Orani', 'Kilit_Pas', 'Top_Kapma', 'Hava_Topu']:
                final_veri[col] = 0
            return final_veri
        except:
            return None

    def transfermarkt_deger_getir(self, oyuncu_ismi):
        print(f"💰 Transfermarkt (Google): '{oyuncu_ismi}' aranıyor...")
        
        try:
            self.driver.get("https://www.google.com")
            
            # Google Arama
            try:
                search_box = self.wait.until(EC.presence_of_element_located((By.NAME, "q")))
                search_box.clear()
                search_box.send_keys(f"{oyuncu_ismi} Transfermarkt") 
                search_box.send_keys(Keys.RETURN)
            except:
                print("❌ Google Arama Kutusu Bulunamadı.")
                return 0
            
            time.sleep(2)

            # İLK SONUCA ZORLA TIKLA (JavaScript ile)
            try:
                # h3 başlığını bul
                ilk_sonuc = self.wait.until(EC.presence_of_element_located((By.CSS_SELECTOR, "h3")))
                # En yakın 'a' (link) etiketine git
                link_element = ilk_sonuc.find_element(By.XPATH, "./..")
                print(f"   🔗 Bulunan Link: {ilk_sonuc.text}")
                
                # JAVASCRIPT CLICK (Engelleri deler geçer)
                self.driver.execute_script("arguments[0].click();", link_element)
            except Exception as e:
                print(f"❌ Tıklama Hatası: {e}")
                return 0
            
            time.sleep(4) # Sayfanın iyice yüklenmesini bekle

            # FİYATI SÖK AL (Regex ile Tüm Sayfayı Tara)
            try:
                tum_metin = self.driver.page_source.lower()
            except:
                tum_metin = ""

            # Kalıp: "15.00 mil. €" veya "15,00 mil. €"
            match_mil = re.search(r"(\d+[.,]?\d*)\s*mil\.", tum_metin)
            match_bin = re.search(r"(\d+[.,]?\d*)\s*bin", tum_metin)

            gercek_deger = 0
            
            if match_mil:
                sayi_str = match_mil.group(1).replace(',', '.')
                if sayi_str.count('.') > 1: sayi_str = sayi_str.rsplit('.', 1)[0]
                gercek_deger = float(sayi_str) * 1_000_000
                print(f"   ✅ Piyasa Değeri: {gercek_deger:,.0f} €")

            elif match_bin:
                sayi_str = match_bin.group(1).replace(',', '.')
                gercek_deger = float(sayi_str) * 1_000
                print(f"   ✅ Piyasa Değeri: {gercek_deger:,.0f} €")
            
            else:
                # Yedek: Belki sadece € simgesi vardır
                match_euro = re.search(r"€\s*(\d+[.,]?\d*)m", tum_metin) # €15.00m formatı
                if match_euro:
                     sayi_str = match_euro.group(1).replace(',', '.')
                     gercek_deger = float(sayi_str) * 1_000_000
                     print(f"   ✅ Piyasa Değeri (Format 2): {gercek_deger:,.0f} €")
                else:
                    print("⚠️ Fiyat bulunamadı (Sayfa kaynağında yok).")
                    return 0

            return int(gercek_deger)

        except Exception as e:
            print(f"❌ Scraper Hatası: {e}")
            return 0

    def _get_text(self, row, stat, is_time=False, is_age=False):
        try:
            text = row.find_element(By.XPATH, f".//td[@data-stat='{stat}']").text
            if is_time: return int(text.replace(',', ''))
            if is_age: return int(text.split('-')[0])
            return int(text)
        except: return 0

    # ARTIK BU FONKSİYON BURADA! 👇
    def kapat(self):
        try:
            self.driver.quit()
        except:
            pass