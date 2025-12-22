import pandas as pd
from sqlalchemy import create_engine

# --- AYARLAR ---
# 1. Dosya adını senin dosyanla eşledim:
csv_dosyasi = 'Final_Veriler_Kalecisiz.csv'
# 2. ŞİFRENİ BURAYA YAZ:
db_sifre = 'oguzhan121734'  # <-- Buraya kendi PostgreSQL şifreni yaz!

# Veritabanı Bağlantısı
baglanti_cumlesi = f'postgresql://postgres:{db_sifre}@localhost:5432/ScoutDB'
engine = create_engine(baglanti_cumlesi)

print("CSV okunuyor...")
df = pd.read_csv(csv_dosyasi)

# --- ADIM 1: TAKIMLARI EKLE (TEAMS) ---
print("Takımlar yükleniyor...")
# CSV'deki 'Takim' ve 'Lig' sütunlarını alıyoruz
unique_teams = df[['Takim', 'Lig']].drop_duplicates().copy()
# Veritabanındaki sütun adlarına çeviriyoruz (team_name, league_name)
unique_teams.columns = ['team_name', 'league_name'] 
unique_teams.to_sql('teams', engine, if_exists='append', index=False)
print("✅ Takımlar eklendi!")

# --- ADIM 2: OYUNCULARI EKLE (PLAYERS) ---
print("Oyuncular yükleniyor...")

# Takım ID'lerini çekelim ki oyuncuları takımlarla eşleştirebilelim
db_teams = pd.read_sql('SELECT team_id, team_name FROM teams', engine)

# CSV ile Veritabanındaki Takımları birleştir (Merge)
# left_on='Takim' (CSV'deki ad), right_on='team_name' (DB'deki ad)
df_merged = pd.merge(df, db_teams, left_on='Takim', right_on='team_name', how='left')

# Players tablosu için gerekli sütunları seçiyoruz
players_data = df_merged[['Oyuncu', 'Ana_Mevki', 'Yas', 'Uyruk', 'team_id', 'Piyasa_Degeri_Euro']].copy()

# Sütun isimlerini veritabanına uygun hale getiriyoruz
players_data.columns = ['full_name', 'position', 'age', 'nationality', 'team_id', 'current_market_value']

players_data.to_sql('players', engine, if_exists='append', index=False)
print("✅ Oyuncular eklendi!")

# --- ADIM 3: İSTATİSTİKLERİ EKLE (PLAYERSTATS) ---
print("İstatistikler yükleniyor...")

# Oyuncu ID'lerini çekelim
db_players = pd.read_sql('SELECT player_id, full_name FROM players', engine)

# İsim üzerinden eşleştirme yapalım
df_final = pd.merge(df, db_players, left_on='Oyuncu', right_on='full_name', how='left')

# İstatistik sütunlarını seçelim
# CSV Başlıkları: 'Gol', 'Asist', 'Oynadigi_Sure_Dk', 'Mac_Sayisi', 'Sari_Kart', 'Kirmizi_Kart'
stats_data = df_final[['player_id', 'Gol', 'Asist', 'Oynadigi_Sure_Dk', 'Mac_Sayisi', 'Sari_Kart', 'Kirmizi_Kart']].copy()

# Veritabanı sütun isimlerine çevirelim
stats_data.columns = ['player_id', 'goals', 'assists', 'minutes_played', 'matches_played', 'yellow_cards', 'red_cards']

# Sezon bilgisini biz ekleyelim
stats_data['season'] = '2024-2025'

stats_data.to_sql('playerstats', engine, if_exists='append', index=False)
print("✅ İstatistikler eklendi! İŞLEM TAMAM.")