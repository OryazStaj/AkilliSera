"""Akıllı Sera IP Kamera & Yapay Zeka Servisi.

Bu modül:
1. IP Kamera (ESP32-CAM / RTSP / HTTP URL) üzerinden veya simülasyon klasöründen anlık görüntü alır.
2. Görüntüyü 'kamera_anlik.jpg' dosyasına üzerine yazarak (overwrite) kaydeder (disk dolmaz).
3. ONNX modelleriyle yaprak hastalığı, domates olgunluğu ve bitki evresini tespit eder.
4. Çıkan sonucu Backend API'sine (POST /api/Analiz/goruntu-sonucu) gönderir.
"""

from __future__ import annotations

import argparse
import random
import shutil
import time
import urllib.request
from pathlib import Path
from analiz import akilli_sera_analiz_et, backende_gonder, VARSAYILAN_BACKEND_URL

BASE_DIR = Path(__file__).resolve().parent
SABIT_FOTOGRAF_YOLU = BASE_DIR / "kamera_anlik.jpg"

# --------------------------------------------------------------------------
# IP KAMERA AYARI:
# Kamera bağlandığında buraya ESP32-CAM veya IP Kamera URL'ini yazabilirsiniz.
# Örnekler: "http://192.168.1.50/cam-hi.jpg", "http://192.168.1.50/capture", "rtsp://..."
# Boş bırakılırsa sistem otomatik olarak simülasyon (örnek fotoğraflar) modunda çalışır.
# --------------------------------------------------------------------------
IP_KAMERA_URL = "" 

# Varsayılan analiz sıklığı (saniye cinsinden)
# Gerçek serada bitki gelişimi için önerilen: 300 saniye (5 dk) veya 1800 saniye (30 dk)
VARSAYILAN_ARALIK_SANIYE = 60


def ornek_fotograflari_bul() -> list[Path]:
    """Test için klasördeki örnek yaprak fotoğraflarını bulur."""
    uzantilar = ("*.jpg", "*.jpeg", "*.png")
    fotograflar = []
    for uzanti in uzantilar:
        for dosya in BASE_DIR.glob(uzanti):
            if dosya.name != "kamera_anlik.jpg":
                fotograflar.append(dosya)
    return fotograflar


def ip_kameradan_kare_al(ip_url: str) -> bool:
    """IP Kamera veya ESP32-CAM üzerinden anlık JPEG karesi çeker."""
    if not ip_url:
        return False

    try:
        print(f"📡 IP Kameraya bağlanılıyor: {ip_url}")
        if ip_url.startswith("http://") or ip_url.startswith("https://"):
            req = urllib.request.Request(ip_url, headers={"User-Agent": "AkilliSera-AI/1.0"})
            with urllib.request.urlopen(req, timeout=5) as response:
                resim_verisi = response.read()
                SABIT_FOTOGRAF_YOLU.write_bytes(resim_verisi)
                print(f"📸 IP Kameradan anlık kare alındı ve kaydedildi: {SABIT_FOTOGRAF_YOLU.name}")
                return True
        else:
            # RTSP veya video akışı ise OpenCV ile yakala
            import cv2
            kamera = cv2.VideoCapture(ip_url)
            if not kamera.isOpened():
                print(f"⚠️ RTSP Akışı açılamadı: {ip_url}")
                return False
            basarili, kare = kamera.read()
            kamera.release()
            if basarili and kare is not None:
                cv2.imwrite(str(SABIT_FOTOGRAF_YOLU), kare)
                print(f"📸 RTSP Akışından anlık kare alındı: {SABIT_FOTOGRAF_YOLU.name}")
                return True
    except Exception as e:
        print(f"⚠️ IP Kameradan görüntü alınamadı ({e}). Simülasyon moduna geçiliyor.")
        return False
    return False


def simule_kare_al(secilen_fotograf: Path | None = None) -> bool:
    """Test fotoğraflarından birini kamera_anlik.jpg üzerine kopyalar."""
    if secilen_fotograf and secilen_fotograf.is_file():
        kaynak = secilen_fotograf
    else:
        ornekler = ornek_fotograflari_bul()
        if not ornekler:
            print("⚠️ Klasörde örnek test fotoğrafı bulunamadı.")
            return False
        kaynak = random.choice(ornekler)

    shutil.copyfile(kaynak, SABIT_FOTOGRAF_YOLU)
    print(f"🔄 Simülasyon: '{kaynak.name}' ➔ '{SABIT_FOTOGRAF_YOLU.name}' olarak güncellendi.")
    return True


def kare_isle_ve_gonder(
    sera_id: int = 1,
    backend_url: str = VARSAYILAN_BACKEND_URL,
    guven_esigi: float = 0.30,
) -> bool:
    """Sabit fotoğraftaki görüntüyü modellerle işler ve backend'e gönderir."""
    if not SABIT_FOTOGRAF_YOLU.is_file():
        print("⚠️ İşlenecek fotoğraf bulunamadı.")
        return False

    print("🧠 Yapay Zeka modelleri çalıştırılıyor...")
    sonuc = akilli_sera_analiz_et(
        fotograf_yolu=SABIT_FOTOGRAF_YOLU,
        sera_id=sera_id,
        guven_esigi=guven_esigi,
    )

    hastalik_sayisi = len(sonuc.get("hastalikDetaylari", []))
    domates_sayisi = len(sonuc.get("domatesDetaylari", []))
    evre = sonuc.get("bitkiEvresi")

    print(f"📊 Tespit Sonucu: Evre={evre}, Hastalık={hastalik_sayisi} adet, Domates={domates_sayisi} adet")
    
    # Backend'e gönder
    return backende_gonder(sonuc, url=backend_url)


def main():
    ayrac = argparse.ArgumentParser(description="Akıllı Sera IP Kamera & Yapay Zeka Entegrasyon Servisi")
    ayrac.add_argument("--ip", type=str, default=IP_KAMERA_URL, help="IP Kamera veya ESP32-CAM URL (örn: http://192.168.1.50/cam-hi.jpg)")
    ayrac.add_argument("--aralik", type=int, default=0, help=f"Fotoğraf çekme sıklığı (saniye). 0 ise tek sefer çalışır. (Örn: {VARSAYILAN_ARALIK_SANIYE})")
    ayrac.add_argument("--fotograf", type=Path, default=None, help="Spesifik bir test fotoğrafı kullanmak için dosya yolu")
    ayrac.add_argument("--sera-id", type=int, default=1, help="Sera ID (Varsayılan: 1)")
    ayrac.add_argument("--url", type=str, default=VARSAYILAN_BACKEND_URL, help="Backend API URL")
    ayrac.add_argument("--guven-esigi", type=float, default=0.30, help="YOLO Güven Eşiği (0-1)")

    argumanlar = ayrac.parse_args()

    print("==================================================")
    print("   🌱 AKILLI SERA - IP KAMERA & AI SERVİSİ       ")
    print("==================================================")
    if argumanlar.ip:
        print(f"📡 Hedef Kamera: {argumanlar.ip}")
    else:
        print("💡 Kamera IP girilmedi: Simülasyon (örnek fotoğraflar) aktif.")

    while True:
        # 1. Fotoğrafı al (IP Kamera veya Simülasyon)
        alinabildi_mi = False
        if argumanlar.ip:
            alinabildi_mi = ip_kameradan_kare_al(argumanlar.ip)

        if not alinabildi_mi:
            simule_kare_al(argumanlar.fotograf)

        # 2. İşle ve Backend'e gönder
        kare_isle_ve_gonder(
            sera_id=argumanlar.sera_id,
            backend_url=argumanlar.url,
            guven_esigi=argumanlar.guven_esigi,
        )

        if argumanlar.aralik <= 0:
            break

        print(f"⏳ {argumanlar.aralik} saniye sonra bir sonraki kare alınacak...\n")
        time.sleep(argumanlar.aralik)


if __name__ == "__main__":
    main()
