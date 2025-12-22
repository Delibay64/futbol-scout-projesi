#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
CLI wrapper for scraper.py - Returns JSON output for C# integration
"""
import sys
import json
from scraper import FutbolScraper

def main():
    if len(sys.argv) < 2:
        print(json.dumps({"error": "Oyuncu ismi gerekli"}))
        sys.exit(1)

    player_name = sys.argv[1]

    try:
        scraper = FutbolScraper(headless=True)

        # FBref'ten istatistikleri çek
        fbref_data = scraper.fbref_veri_getir(player_name)

        if fbref_data is None:
            scraper.kapat()
            print(json.dumps({"error": f"'{player_name}' oyuncusu FBref'te bulunamadı"}))
            sys.exit(1)

        # Transfermarkt'tan piyasa değerini çek
        market_value = scraper.transfermarkt_deger_getir(player_name)

        # Scraper'ı kapat
        scraper.kapat()

        # Sonuçları birleştir
        result = {
            "full_name": fbref_data.get('Oyuncu', player_name),
            "age": fbref_data.get('Yas', 25),
            "position": "Forvet",  # Varsayılan
            "nationality": "Bilinmiyor",  # FBref'ten çekilemiyor
            "team_name": fbref_data.get('Takim', 'Bilinmiyor'),
            "league_name": fbref_data.get('Lig', 'Bilinmiyor'),
            "current_market_value": market_value,
            "stats": {
                "goals": fbref_data.get('Gol', 0),
                "assists": fbref_data.get('Asist', 0),
                "matches_played": fbref_data.get('Mac_Sayisi', 0),
                "minutes_played": fbref_data.get('Oynadigi_Sure_Dk', 0),
                "yellow_cards": 0,
                "red_cards": 0
            }
        }

        # JSON olarak çıktı ver
        print(json.dumps(result, ensure_ascii=False))

    except Exception as e:
        print(json.dumps({"error": str(e)}))
        sys.exit(1)

if __name__ == "__main__":
    main()