# 📷 Görüntü İşleme & Kamera Servisi

Bu modül; ONNX formatındaki yapay zeka modellerini kullanarak yaprak hastalıklarını, domates olgunluğunu ve bitki evresini (`Tohum`, `Filiz`, `Olgun`) tespit eder ve sonuçları doğrudan Backend API'sine (`POST /api/Analiz/goruntu-sonucu`) iletir.

---

## 🛠️ Kurulum

```powershell
cd 1_AI_Python
pip install -r requirements.txt
```

---

## 🚀 Çalıştırma Yöntemleri

### 1. Otomatik Kamera / Simülasyon Servisi (`kamera_servisi.py` - Önerilen)

Bu servis, anlık görüntüyü `kamera_anlik.jpg` dosyasına **üzerine yazarak (overwrite)** kaydeder, analiz eder ve sonucu Backend'e gönderir:

* **Tek Seferlik Simülasyon Testi:**
  ```powershell
  python kamera_servisi.py
  ```

* **Sürekli Canlı Akış (Örn: Her 5 saniyede bir yeni kare işle & gönder):**
  ```powershell
  python kamera_servisi.py --dongu 5
  ```

* **Fiziksel USB Web Kameradan Çekim Yapmak:**
  ```powershell
  python kamera_servisi.py --kamera-id 0 --dongu 10
  ```

---

### 2. Bağımsız Analiz Scripti (`analiz.py`)

Spesifik bir görüntüyü analiz etmek veya çıktısını JSON dosyasına kaydetmek için:

* **Ekrana JSON Basma:**
  ```powershell
  python analiz.py "test_yapragi.jpg"
  ```

* **Analiz Edip Doğrudan Backend'e Gönderme:**
  ```powershell
  python analiz.py "test_yapragi.jpg" --gonder
  ```

---

## 📡 Backend Entegrasyon Sözleşmesi (`POST /api/Analiz/goruntu-sonucu`)

Analiz sonucu Backend'e şu JSON formatında iletilir:

```json
{
  "seraId": 1,
  "bitkiEvresi": "Olgun",
  "yaprakTespitEdildiMi": true,
  "domatesTespitEdildiMi": true,
  "hastalikDetaylari": [
    { "hastalikAdi": "Erken Yaniklik", "guvenSkoru": 92.4 }
  ],
  "domatesDetaylari": [
    { "durum": "Olgun", "guvenSkoru": 88.7 }
  ],
  "fotografYolu": "kamera_anlik.jpg",
  "analizZamani": "2026-08-17T19:00:00+00:00",
  "guvenEsigi": 30.0,
  "aciklama": "Bitkide domates tespit edildi."
}
```

* Backend bu veriyi aldığında:
  1. Hastalık tespit edildiyse `Bitki_Hastalik` ve `Bildirim` tablolarına kaydeder.
  2. `Sera_Durum` üzerindeki `AktifEvreID` alanını günceller.
  3. SignalR (`ReceivePlantAnalysis`) üzerinden Frontend web arayüzüne anlık uyarı fırlatır.
