import os
import json
import cv2
from ultralytics import YOLO

# ---------------- 1. MODELLERİ YÜKLE ----------------
hastalik_model_yolu = "model.onnx"        
domates_model_yolu = "model_domates.onnx"  

print("Modeller yükleniyor...")
hastalik_modeli = YOLO(hastalik_model_yolu, task='detect')
domates_modeli = YOLO(domates_model_yolu, task='detect')


def akilli_sera_analiz_et(fotograf_yolu):
    """
    Görseli önce domates modeli, ardından hastalık modeli ile tarar:
    1. Domates tespit edildiyse -> OLGUN
    2. Domates yok ama yaprak/bitki varsa -> FİLİZ (FİDE)
    3. Hiçbiri yoksa -> TOHUM
    Sonucu C# Backend ekibinin tüketebileceği JSON formatında döner.
    """
    if not os.path.exists(fotograf_yolu):
        return json.dumps({"hata": f"'{fotograf_yolu}' dosyası bulunamadı!"}, ensure_ascii=False)

    image = cv2.imread(fotograf_yolu)
    
    # --- ADIM 1: ÖNCE DOMATES MODELİ İLE KONTROL (Öncelik Olgunlukta) ---
    domates_sonuclari = domates_modeli(fotograf_yolu, conf=0.30)  
    domates_bulundu = False
    domates_detaylari = []

    for result in domates_sonuclari:
        for box in result.boxes:
            domates_bulundu = True
            class_id = int(box.cls[0])
            class_name = domates_modeli.names[class_id] if domates_modeli.names else f"Sinif_{class_id}"
            confidence = float(box.conf[0])
            
            domates_detaylari.append({
                "durum": class_name,  # ripe, unripe, rotten vb.
                "guven_skoru": confidence  # Ham skor korundu
            })

    # Eğer domates bulunduysa direkt OLGUN kabul et
    if domates_bulundu:
        # İsteğe bağlı olarak arka planda yaprak/hastalık da taranabilir
        hastalik_sonuclari = hastalik_modeli(fotograf_yolu, conf=0.30)
        tespit_edilen_hastaliklar = []
        for result in hastalik_sonuclari:
            for box in result.boxes:
                class_id = int(box.cls[0])
                class_name = hastalik_modeli.names[class_id] if hastalik_modeli.names else f"Sinif_{class_id}"
                confidence = float(box.conf[0])
                tespit_edilen_hastaliklar.append({"hastalik": class_name, "guven_skoru": confidence})

        sonuc_veri = {
            "bitki_evresi": "Olgun",
            "yaprak_tespit_edildi_mi": len(tespit_edilen_hastaliklar) > 0,
            "domates_tespit_edildi_mi": True,
            "hastalik_detaylari": tespit_edilen_hastaliklar,
            "domates_detaylari": domates_detaylari,
            "aciklama": "Bitkide domates tespit edildi, olgunluk aşamasında."
        }
        return json.dumps(sonuc_veri, indent=4, ensure_ascii=False)


    # --- ADIM 2: DOMATES YOKSA YAPRAK / HASTALIK MODELİ İLE KONTROL ---
    hastalik_sonuclari = hastalik_modeli(fotograf_yolu, conf=0.30)  
    yaprak_bulundu = False
    tespit_edilen_hastaliklar = []

    for result in hastalik_sonuclari:
        for box in result.boxes:
            yaprak_bulundu = True
            class_id = int(box.cls[0])
            class_name = hastalik_modeli.names[class_id] if hastalik_modeli.names else f"Sinif_{class_id}"
            confidence = float(box.conf[0])
            
            tespit_edilen_hastaliklar.append({
                "hastalik": class_name,
                "guven_skoru": confidence  # Ham skor korundu
            })

    # Yaprak bulunduysa -> FİLİZ (FİDE)
    if yaprak_bulundu:
        sonuc_veri = {
            "bitki_evresi": "Filiz",
            "yaprak_tespit_edildi_mi": True,
            "domates_tespit_edildi_mi": False,
            "hastalik_detaylari": tespit_edilen_hastaliklar,
            "domates_detaylari": [],
            "aciklama": "Bitkide yaprak tespit edildi ancak domates yok, filiz/fide aşamasında."
        }
        return json.dumps(sonuc_veri, indent=4, ensure_ascii=False)


    # --- ADIM 3: NE DOMATES NE DE YAPRAK VARSA -> TOHUM ---
    sonuc_veri = {
        "bitki_evresi": "Tohum",
        "yaprak_tespit_edildi_mi": False,
        "domates_tespit_edildi_mi": False,
        "hastalik_detaylari": [],
        "domates_detaylari": [],
        "aciklama": "Ekranda ne domates ne de yaprak tespit edilemedi, bitki tohum aşamasında."
    }
    return json.dumps(sonuc_veri, indent=4, ensure_ascii=False)


# ---------------- TEST ÇALIŞTIRMASI ----------------
if __name__ == "__main__":
    test_fotografi = "images (6).jpg" 
    
    json_cikti = akilli_sera_analiz_et(test_fotografi)
    
    print("\n--- BACKEND'E (C#) GÖNDERİLECEK JSON ÇIKTISI ---")
    print(json_cikti)