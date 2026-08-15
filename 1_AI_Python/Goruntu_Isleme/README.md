# Görüntü İşleme

`analiz.py`, hastalık ve domates ONNX modellerini kullanarak bir görüntüyü analiz eder. Çıktı UTF-8 kodlu, geçerli JSON'dur ve backend'e gönderilecek sözleşmenin aday formatıdır.

## Kurulum

Python 3.11 önerilir. Gerekli paketler:

```powershell
pip install ultralytics opencv-python
```

## Çalıştırma

Bu klasördeyken:

```powershell
python analiz.py "images (6).jpg" --sera-id 1 --cikti analiz-sonucu.json
```

`--guven-esigi` parametresi `0–1` arasındadır; varsayılan değer `0.30`dur.

## Çıktı sözleşmesi

Ana alanlar şunlardır:

```json
{
  "seraId": 1,
  "bitkiEvresi": "Olgun",
  "hastalikDetaylari": [
    { "hastalikAdi": "ornek_hastalik", "guvenSkoru": 87.5 }
  ],
  "domatesDetaylari": [
    { "durum": "ripe", "guvenSkoru": 91.2 }
  ],
  "fotografYolu": "C:\\...\\gorsel.jpg",
  "analizZamani": "2026-08-15T12:00:00+00:00"
}
```

`guvenSkoru` ve `guvenEsigi` yüzde formatındadır (`0–100`). `hastalikOrani` alanı veritabanında kullanılacaksa bu yüzde formatı backend ve veritabanı ekibi tarafından kabul edilmelidir.

## Backend entegrasyon durumu

Mevcut C# API'de görüntü analizi sonucu kabul eden endpoint bulunmuyor. `POST /api/Telemetry` sensör telemetrisi içindir; görüntü analizi sonucu bu endpoint'e gönderilmemelidir.

Backend ekibinin ayrı bir endpoint oluşturması gerekir; önerilen adres `POST /api/Analiz/goruntu-sonucu`dur. Bu endpoint aşağıdaki alanları kabul eden bir DTO yayımlamalıdır:

- `seraId`
- `bitkiEvresi`
- `hastalikDetaylari[].hastalikAdi`
- `hastalikDetaylari[].guvenSkoru` (yüzde, `0–100`)
- `domatesDetaylari`
- `fotografYolu`
- `analizZamani`
- `guvenEsigi` (yüzde, `0–100`)

Endpoint/DTO bu sözleşme ile yayınlanana kadar script HTTP isteği yapmaz. Bu yaklaşım, yanlış endpoint'e veri gönderilmesini ve AI sonucunun geçersiz sensör verisi olarak yorumlanmasını önler.
