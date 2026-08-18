"""Akıllı Sera görüntü analizi.

Bu modül bir görüntü için yaprak hastalığı, domates olgunluğu ve bitki evresi
tespitlerini yapar, JSON uyumlu sözlük üretir ve isteğe bağlı olarak doğrudan
Backend API'sine (POST /api/Analiz/goruntu-sonucu) gönderir.
"""

from __future__ import annotations

import argparse
import json
import ssl
import urllib.request
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

import cv2
from ultralytics import YOLO
from ultralytics.utils import LOGGER

BASE_DIR = Path(__file__).resolve().parent
HASTALIK_MODEL_YOLU = BASE_DIR / "model.onnx"
DOMATES_MODEL_YOLU = BASE_DIR / "model_domates.onnx"
VARSAYILAN_FOTOGRAF = BASE_DIR / "kamera_anlik.jpg"
VARSAYILAN_BACKEND_URL = "https://localhost:7266/api/Analiz/goruntu-sonucu"
VARSAYILAN_GUVEN_ESIGI = 0.30

# CLI çıktısı başka sistemler tarafından doğrudan JSON olarak okunabilsin.
LOGGER.setLevel("ERROR")


def model_yukle() -> tuple[YOLO, YOLO]:
    """Model dosyalarının varlığını doğrular ve modelleri yükler."""
    eksik_modeller = [
        yol.name
        for yol in (HASTALIK_MODEL_YOLU, DOMATES_MODEL_YOLU)
        if not yol.is_file()
    ]
    if eksik_modeller:
        raise FileNotFoundError("Model dosyası bulunamadı: " + ", ".join(eksik_modeller))

    return (
        YOLO(str(HASTALIK_MODEL_YOLU), task="detect"),
        YOLO(str(DOMATES_MODEL_YOLU), task="detect"),
    )


def _tespitleri_oku(sonuclar: Any, model: YOLO, alan_adi: str) -> list[dict[str, Any]]:
    """YOLO sonuçlarını JSON serileştirilebilir, kararlı bir listeye dönüştürür."""
    tespitler: list[dict[str, Any]] = []
    for sonuc in sonuclar:
        if sonuc.boxes is None:
            continue
        for kutu in sonuc.boxes:
            sinif_id = int(kutu.cls[0])
            sinif_adi = model.names.get(sinif_id, f"Sinif_{sinif_id}")
            tespitler.append(
                {
                    alan_adi: str(sinif_adi),
                    # Backend için yüzde formatı kullanılır: 0-100.
                    "guvenSkoru": round(float(kutu.conf[0]) * 100, 2),
                }
            )
    return tespitler


def akilli_sera_analiz_et(
    fotograf_yolu: str | Path,
    sera_id: int | None = None,
    guven_esigi: float = VARSAYILAN_GUVEN_ESIGI,
) -> dict[str, Any]:
    """Görüntüyü analiz eder ve JSON'a çevrilebilir sonuç sözlüğü döndürür.

    Evre kuralı: domates varsa ``Olgun``; domates yok ama hastalık/yaprak
    tespiti varsa ``Filiz``; hiçbir tespit yoksa ``Tohum``.
    """
    if not 0 < guven_esigi <= 1:
        raise ValueError("guven_esigi 0 ile 1 arasında olmalıdır.")
    if sera_id is not None and sera_id <= 0:
        raise ValueError("sera_id pozitif bir tam sayı olmalıdır.")

    fotograf = Path(fotograf_yolu).expanduser().resolve()
    if not fotograf.is_file():
        raise FileNotFoundError(f"Görüntü dosyası bulunamadı: {fotograf}")
    if cv2.imread(str(fotograf)) is None:
        raise ValueError(f"Görüntü okunamadı: {fotograf}")

    hastalik_modeli, domates_modeli = model_yukle()
    domatesler = _tespitleri_oku(
        domates_modeli(str(fotograf), conf=guven_esigi, verbose=False),
        domates_modeli,
        "durum",
    )
    hastaliklar = _tespitleri_oku(
        hastalik_modeli(str(fotograf), conf=guven_esigi, verbose=False),
        hastalik_modeli,
        "hastalikAdi",
    )

    if domatesler:
        bitki_evresi = "Olgun"
        aciklama = "Bitkide domates tespit edildi."
    elif hastaliklar:
        bitki_evresi = "Filiz"
        aciklama = "Yaprak/bitki tespiti yapıldı, domates tespit edilmedi."
    else:
        bitki_evresi = "Tohum"
        aciklama = "Domates veya yaprak/bitki tespiti yapılamadı."

    return {
        "seraId": sera_id if sera_id is not None else 1,
        "bitkiEvresi": bitki_evresi,
        "yaprakTespitEdildiMi": bool(hastaliklar),
        "domatesTespitEdildiMi": bool(domatesler),
        "hastalikDetaylari": hastaliklar,
        "domatesDetaylari": domatesler,
        "fotografYolu": fotograf.name,
        "analizZamani": datetime.now(timezone.utc).isoformat(),
        "guvenEsigi": round(guven_esigi * 100, 2),
        "aciklama": aciklama,
    }


def backende_gonder(analiz_sonucu: dict[str, Any], url: str = VARSAYILAN_BACKEND_URL) -> bool:
    """Analiz sonucunu Backend API'sine HTTP POST ile gönderir."""
    try:
        veri = json.dumps(analiz_sonucu, ensure_ascii=False).encode("utf-8")
        istek = urllib.request.Request(
            url,
            data=veri,
            headers={"Content-Type": "application/json; charset=utf-8"},
            method="POST",
        )
        ctx = ssl.create_default_context()
        ctx.check_hostname = False
        ctx.verify_mode = ssl.CERT_NONE

        with urllib.request.urlopen(istek, context=ctx, timeout=10) as yanit:
            cevap = yanit.read().decode("utf-8")
            print(f"✅ Backend'e başarıyla gönderildi: {cevap}")
            return True
    except Exception as e:
        print(f"⚠️ Backend'e gönderilemedi: {e}")
        return False


def _argumanlari_oku() -> argparse.Namespace:
    ayrac = argparse.ArgumentParser(description="Akıllı Sera görüntü analizi")
    ayrac.add_argument(
        "fotograf",
        nargs="?",
        type=Path,
        default=VARSAYILAN_FOTOGRAF,
        help=f"Analiz edilecek görüntü (Varsayılan: {VARSAYILAN_FOTOGRAF.name})",
    )
    ayrac.add_argument("--sera-id", type=int, default=1, help="İlgili sera ID (Varsayılan: 1)")
    ayrac.add_argument(
        "--guven-esigi",
        type=float,
        default=VARSAYILAN_GUVEN_ESIGI,
        help="YOLO güven eşiği (0-1, varsayılan: 0.30)",
    )
    ayrac.add_argument(
        "--cikti",
        type=Path,
        default=None,
        help="JSON sonucunun yazılacağı dosya",
    )
    ayrac.add_argument(
        "--gonder",
        action="store_true",
        help="Analiz sonucunu otomatik olarak Backend API'sine POST eder",
    )
    ayrac.add_argument(
        "--url",
        type=str,
        default=VARSAYILAN_BACKEND_URL,
        help=f"Backend endpoint URL (Varsayılan: {VARSAYILAN_BACKEND_URL})",
    )
    return ayrac.parse_args()


def main() -> int:
    argumanlar = _argumanlari_oku()
    try:
        sonuc = akilli_sera_analiz_et(
            argumanlar.fotograf,
            sera_id=argumanlar.sera_id,
            guven_esigi=argumanlar.guven_esigi,
        )
    except (FileNotFoundError, ValueError) as hata:
        print(json.dumps({"hata": str(hata)}, ensure_ascii=False))
        return 1

    sonuc_json = json.dumps(sonuc, ensure_ascii=False, indent=2)
    if argumanlar.cikti:
        argumanlar.cikti.parent.mkdir(parents=True, exist_ok=True)
        argumanlar.cikti.write_text(sonuc_json + "\n", encoding="utf-8")
    
    print(sonuc_json)

    if argumanlar.gonder:
        backende_gonder(sonuc, url=argumanlar.url)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
