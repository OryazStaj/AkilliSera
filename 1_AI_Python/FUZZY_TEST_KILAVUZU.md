# 🧪 Fuzzy API Terminal Test Kılavuzu

Dosya oluşturmadan, sadece terminal kullanarak Fuzzy sistemini test etmek için adım adım rehber.

---

## 🖥️ Adım 1 — Flask Sunucusunu Başlat

**1. Terminal Penceresi** açın ve çalıştırın:

```powershell
cd 1_AI_Python
python Bulanik_Mantik/fuzzy_sistem.py
```

Sunucu başarıyla başlarsa şunu görürsünüz:

```
 * Running on http://0.0.0.0:5000
 * Running on http://127.0.0.1:5000
```

> ⚠️ Bu terminali **kapatmayın** — sunucu çalışmaya devam etsin.

---

## 🔬 Adım 2 — İkinci Terminalde Test İsteği Gönder

**Yeni bir terminal** açın (birincisini kapatmadan).

### PowerShell ile Test (Windows Önerilen)

```powershell
$body = @{
    bitki = @{
        bitkiAdi          = "Domates"
        evreAdi           = "Fide"
        minToprakNemi     = 60
        maxToprakNemi     = 75
        minOrtamNemi      = 65
        maxOrtamNemi      = 75
        gunduzMinSicaklik = 21
        gunduzMaxSicaklik = 26
        geceMinSicaklik   = 16
        geceMaxSicaklik   = 19
    }
    anlikVeriler = @{
        toprakNemi = 38
        ortamNemi  = 45
        sicaklik   = 31
    }
    zaman = @{
        saat = 14
    }
} | ConvertTo-Json -Depth 5

Invoke-WebRequest -Uri "http://localhost:5000/api/fuzzy/calculate" `
    -Method POST `
    -ContentType "application/json" `
    -Body $body | Select-Object -ExpandProperty Content
```

### curl ile Test (Alternatif)

```bash
curl -X POST http://localhost:5000/api/fuzzy/calculate \
  -H "Content-Type: application/json" \
  -d "{\"bitki\":{\"bitkiAdi\":\"Domates\",\"evreAdi\":\"Fide\",\"minToprakNemi\":60,\"maxToprakNemi\":75,\"minOrtamNemi\":65,\"maxOrtamNemi\":75,\"gunduzMinSicaklik\":21,\"gunduzMaxSicaklik\":26,\"geceMinSicaklik\":16,\"geceMaxSicaklik\":19},\"anlikVeriler\":{\"toprakNemi\":38,\"ortamNemi\":45,\"sicaklik\":31},\"zaman\":{\"saat\":14}}"
```

---

## ✅ Beklenen Çıktı

Başarılı bir istekte şuna benzer bir JSON döner:

```json
{
    "bitki": "Domates",
    "evre": "Fide",
    "kararlar": {
        "sulama": {
            "sure": 22.5,
            "karar": "fazla sulama"
        },
        "havalandirma": {
            "seviye": 68.3,
            "karar": "yuksek fan"
        },
        "isitma": {
            "seviye": 26.0,
            "karar": "isi_yukselt"
        }
    }
}
```

---

## 🎯 Farklı Senaryolar ile Test

Aşağıdaki `anlikVeriler` değerlerini değiştirerek farklı kararları test edebilirsiniz:

| Senaryo | toprakNemi | ortamNemi | sicaklik | saat | Beklenen Sulama |
|---------|-----------|-----------|---------|------|----------------|
| Her şey ideal | 67 | 70 | 23 | 14 | sulama yok |
| Çok kuru, sıcak | 20 | 30 | 35 | 14 | cok fazla sulama |
| Islak, soğuk | 85 | 90 | 10 | 3 | sulama yok |
| Gece, normal | 65 | 68 | 17 | 22 | sulama yok |

---

## ❌ Hata Durumları

| Hata | Sebep | Çözüm |
|------|-------|-------|
| `Connection refused` | Flask sunucusu çalışmıyor | Adım 1'i yapın |
| `400 - JSON verisi bekleniyor` | İstek gövdesi boş | `-ContentType "application/json"` ekleyin |
| `500 - hata: ...` | JSON alanı eksik/yanlış | `bitki`, `anlikVeriler`, `zaman` alanlarını kontrol edin |
| `ModuleNotFoundError` | Python paketi eksik | `pip install -r requirements.txt` çalıştırın |
