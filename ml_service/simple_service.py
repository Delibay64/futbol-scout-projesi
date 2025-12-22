# -*- coding: utf-8 -*-
from flask import Flask, request, jsonify
import joblib
import pandas as pd
import numpy as np
import os
import requests
from bs4 import BeautifulSoup
import re

app = Flask(__name__)

# Model yukle
MODEL_PATH = os.path.join(os.path.dirname(__file__), 'models', 'futbol_zeka_sistemi.pkl')
print(f"Model yukleniyor: {MODEL_PATH}")

try:
    system = joblib.load(MODEL_PATH)
    model = system['model']
    team_map = system['team_map']
    league_map = system['league_map']
    global_mean = system['global_mean']
    training_columns = system['training_columns']
    print("Model basariyla yuklendi!")
except Exception as e:
    print(f"HATA: Model yuklenemedi - {e}")
    model = None

# Mevki map
mevki_map = {
    'kaleci': 0, 'stoper': 1, 'sol bek': 2, 'sag bek': 3,
    'on libero': 4, 'merkez ortasaha': 5, 'ofansif ortasaha': 6,
    'sol kanat': 7, 'sag kanat': 8, 'forvet': 9, 'santrafor': 9
}

ayak_map = {'sag': 0, 'sol': 1, 'her ikisi': 2, 'right': 0, 'left': 1, 'both': 2}

def temizle(text):
    if not isinstance(text, str):
        return str(text)
    return text.lower().replace('ğ', 'g').replace('ü', 'u').replace('ş', 's').replace('ı', 'i').replace('ö', 'o').replace('ç', 'c').strip()

@app.route('/predict', methods=['POST'])
def predict():
    if model is None:
        return jsonify({'status': 'error', 'mesaj': 'Model yuklu degil!'}), 500

    try:
        data = request.json
        print(f"Tahmin istegi: {data.get('Oyuncu')}")

        df = pd.DataFrame([data])

        takim = df.iloc[0].get('Takim', '')
        lig = df.iloc[0].get('Lig', '')
        df['Takim_Gucu'] = team_map.get(takim, global_mean)
        df['Lig_Gucu'] = league_map.get(lig, global_mean)

        raw_mevki = df.iloc[0].get('Ana_Mevki', '')
        clean_mevki = temizle(raw_mevki)
        df['Ana_Mevki'] = mevki_map.get(clean_mevki, 5)

        raw_ayak = df.iloc[0].get('Ayak', '')
        clean_ayak = temizle(raw_ayak)
        df['Ayak'] = ayak_map.get(clean_ayak, 0)

        model_input = df.reindex(columns=training_columns, fill_value=0)
        for col in model_input.columns:
            model_input[col] = pd.to_numeric(model_input[col], errors='coerce')
        model_input = model_input.fillna(0)

        log_pred = model.predict(model_input)
        price_pred = int(np.expm1(log_pred)[0])

        return jsonify({'status': 'success', 'tahmini_deger': price_pred})

    except Exception as e:
        print(f"Tahmin hatasi: {e}")
        return jsonify({'status': 'error', 'message': str(e)}), 500

@app.route('/scrape_player', methods=['POST'])
def scrape_player():
    try:
        data = request.json
        player_name = data.get('name')
        print(f"Transfermarkt'ta araniyor: {player_name}")

        headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'}

        # Arama yap
        search_url = f"https://www.transfermarkt.com.tr/schnellsuche/ergebnis/schnellsuche?query={player_name}"
        response = requests.get(search_url, headers=headers)
        soup = BeautifulSoup(response.content, 'html.parser')

        player_link_tag = soup.select_one('.items .hauptlink a')
        if not player_link_tag:
            return jsonify({'status': 'error', 'message': 'Oyuncu bulunamadi!'}), 404

        player_url = "https://www.transfermarkt.com.tr" + player_link_tag['href']
        print(f"Profil bulundu: {player_url}")

        # Detay sayfasi
        resp_player = requests.get(player_url, headers=headers)
        soup_p = BeautifulSoup(resp_player.content, 'html.parser')

        # Bilgileri cek
        team = "Bilinmiyor"
        team_tag = soup_p.select_one('.data-header__club a')
        if team_tag:
            team = team_tag.text.strip()

        position = "Merkez Ortasaha"
        pos_box = soup_p.find('li', class_='data-header__label')
        if pos_box and 'Mevki:' in pos_box.text:
            position = pos_box.find('span').text.strip()
        else:
            alt_pos = soup_p.select_one('.detail-position__position')
            if alt_pos:
                position = alt_pos.text.strip()

        # Yas
        age = 25
        age_tag = soup_p.select_one('.data-header__birth-date .data-header__content')
        if not age_tag:
            age_search = soup_p.find(string=lambda t: t and "Yas:" in t)
            if age_search:
                age_tag = age_search.find_next('span')

        if age_tag:
            raw_age_text = age_tag.text.strip()
            match = re.search(r'\((\d+)\)', raw_age_text)
            if match:
                age = int(match.group(1))
            elif raw_age_text.isdigit():
                age = int(raw_age_text)

        # Uyruk
        nationality = "Dunya"
        nat_label = soup_p.find(string=lambda t: t and "Uyruk:" in t)
        if nat_label:
            nat_img = nat_label.find_next('img', class_='flaggenrahmen')
            if nat_img:
                nationality = nat_img.get('title', 'Dunya')
        else:
            nat_tag = soup_p.select_one('.data-header__content img.flaggenrahmen')
            if nat_tag:
                nationality = nat_tag.get('title', 'Dunya')

        # Piyasa degeri
        market_value = 0
        value_tag = soup_p.select_one('.data-header__market-value-wrapper')
        if value_tag:
            raw_val = value_tag.text.strip().split('€')[0].strip()
            raw_val = raw_val.replace('mil.', '').replace('bin', '').replace('€', '').strip()
            raw_val = raw_val.replace(',', '.')
            try:
                val_float = float(raw_val)
                if 'bin' in value_tag.text:
                    market_value = val_float * 1000
                else:
                    market_value = val_float * 1000000
            except:
                market_value = 0

        # Istatistikler
        goals = 0
        assists = 0
        matches = 0
        stats_table = soup_p.select_one('#leistungsdaten_table')
        if stats_table:
            headers_list = stats_table.select('thead th')
            goal_idx = -1
            assist_idx = -1
            match_idx = -1

            for i, th in enumerate(headers_list):
                header_text = th.text.strip().lower()
                img = th.select_one('img')
                if img and img.get('title'):
                    header_text = img['title'].lower()

                if 'maclar' in header_text or 'matches' in header_text:
                    match_idx = i
                if 'gol' in header_text and 'yenen' not in header_text:
                    goal_idx = i
                if 'asist' in header_text:
                    assist_idx = i

            footers = stats_table.select('tfoot td')
            if footers:
                if match_idx != -1 and len(footers) > match_idx:
                    matches = int(footers[match_idx].text.replace('-', '0').replace('.', ''))
                if goal_idx != -1 and len(footers) > goal_idx:
                    goals = int(footers[goal_idx].text.replace('-', '0').replace('.', ''))
                if assist_idx != -1 and len(footers) > assist_idx:
                    assists = int(footers[assist_idx].text.replace('-', '0').replace('.', ''))

        scraped_data = {
            'status': 'success',
            'FullName': player_name,
            'TeamName': team,
            'Position': position,
            'Age': age,
            'Nationality': nationality,
            'CurrentMarketValue': int(market_value),
            'Goals': goals,
            'Assists': assists,
            'MatchesPlayed': matches,
            'MinutesPlayed': matches * 90
        }

        print(f"Basarili: Uyruk={nationality}, Yas={age}, Deger={market_value}")
        return jsonify(scraped_data)

    except Exception as e:
        print(f"Scrape hatasi: {e}")
        import traceback
        traceback.print_exc()
        return jsonify({'status': 'error', 'message': str(e)}), 500

if __name__ == '__main__':
    print("Flask servisi 5000 portunda calisiyor...")
    app.run(port=5000, debug=False)
