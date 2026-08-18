import numpy as np #sayısal işlem ve dizilerde çalışmak için kullanılır. np kısmı kısaltma takma isimdir. kullanılacak değişkenin aralığını ilgilendirir.
import skfuzzy as fuzz # bu fuzzy kütüphanesini içeri aktarır fuzz yine kısaltma takma addır. bununla üyelik fonksiyonlarını oluşturabiliriz.
from skfuzzy import control as ctrl #control bulanık mantık sistemi kurmaya yarar.girdi-çıktılar , kurallar ve en sonunda bütün kuralları birleştirir.
import json
#eğer backend evreyi seçip o değerleri bana vermezse :
#evre = test_json["evreAdi"]
#optimal = test_json["optimalDegerler"][evre] kullanıp test jsonda tüm evre ve min maxları gelicek.
test_json = {
    "bitki": {
        "bitkiAdi": "Domates",
        "evreAdi": "Vejetatif",

        "minToprakNemi": 45,
        "maxToprakNemi": 70,

        "minOrtamNemi": 60,
        "maxOrtamNemi": 80,

        "gunduzMinSicaklik": 20,
        "gunduzMaxSicaklik": 28,

        "geceMinSicaklik": 16,
        "geceMaxSicaklik": 20
    },

    "anlikVeriler": {
        "toprakNemi": 38,
        "ortamNemi": 45,
        "sicaklik": 31
    },

    "zaman": {
        "saat": 14
    }
}

def hesapla(json_veri):
    #aşağıda jsondaki verileri allıyoruz.
    bitki = json_veri["bitki"] #burada optimal değerler var olan bitki hakkında:
    anlik = json_veri["anlikVeriler"] #anlık sensör ve kamera değerleri var:
    zaman = json_veri["zaman"] #bu saat gündüz gece için:

    #şimdi okuduğumuz verileri sabit değerlere eşitlememek için parametrk şekilde kullanıyorum:
    min_toprak = bitki["minToprakNemi"] #ilgili bitlkinin optimal min toprak nemi eşitlendi.
    max_toprak = bitki["maxToprakNemi"]

    min_ortam_nemi = bitki["minOrtamNemi"]
    max_ortam_nemi = bitki["maxOrtamNemi"]

    gunduz_min_sicaklik = bitki["gunduzMinSicaklik"]
    gunduz_max_sicaklik = bitki["gunduzMaxSicaklik"]

    gece_min_sicaklik = bitki["geceMinSicaklik"]
    gece_max_sicaklik = bitki["geceMaxSicaklik"]

    #aşağıda anlık değerler var onları eşitledim değişkene:
    #şimdi sensör toprak nemini 42 ölçerse o değer gelir buraya elle bir şey değiştirmeye gerek yok.
    toprak_nemi = anlik["toprakNemi"]
    ortam_nemi = anlik["ortamNemi"]
    sicaklik = anlik["sicaklik"]

    #saati de okuyup değişkene bağlıyorum:
    saat = zaman["saat"]
    #gündüz gece için belirlenen işlem buna göre sıcaklık kullanılacak:
    if 6 <= saat < 18:
        gunduz = True
    else:
        gunduz = False
    #şimdi neyse onu getirecek kod kısmım:
    if gunduz:
        min_sicaklik = gunduz_min_sicaklik
        max_sicaklik = gunduz_max_sicaklik
    else:
        min_sicaklik = gece_min_sicaklik
        max_sicaklik = gece_max_sicaklik

    #şimdi bu aşağıdaki oranları arkadaşlarla konuşup değiştirebiliriz ama şimdilik bu oranlara göre işlem kendini kuru ıslak gibi değerlendirecek
    KURU_ORAN = 0.50 #min toprak neminin 0.50 kadar altında çok kuru kabul edilecek
    ISLAK_PAY = 15 #max toprak neminin 15 üstü ıslak kabul edişecek ama bu kısımalrı yüzdeliğe değiştirebilirim oranları arkadaşlarla tartışabilirim makaleden de bakabilirim.
    SICAKLIK_PAY = 5 #optimal sıcaklık aralığı 5 derece dışında düşük yüksek diye belirleneek.
    #3 tane çıktı sonucumuz olacak SULAMA-HAVALANDIRMA-ISITMA
    # 1.si yani SULAMA İÇİN İŞLEMLER.
    #toprak nemi aralığı bunu da değiştiricem ama örnek kullanım için:
    #şimdi toprak nemi için üyelik fonksiyonlarını oluşturcam: ÇOK KURU-KURU-İDEAL-ISLAK olacak
    toprak = ctrl.Antecedent(
        np.arange(0, 101, 1),
        "toprak"
    ) #0-100 arası birer birer kabul

    # Optimal aralığın genişliği
    toprak_aralik = max_toprak - min_toprak

    # Çok kuru
    toprak["cok_kuru"] = fuzz.trapmf(
        toprak.universe,
        [
            0,
            0,
            max(0, min_toprak - toprak_aralik),
            min_toprak
        ]
    )

    # Kuru
    toprak["kuru"] = fuzz.trimf(
        toprak.universe,
        [
            max(0, min_toprak - toprak_aralik),
            min_toprak,
            (min_toprak + max_toprak) / 2
        ]
    )

    # İdeal
    toprak["ideal"] = fuzz.trapmf(
        toprak.universe,
        [
            min_toprak,
            min_toprak,
            max_toprak,
            max_toprak
        ]
    )

    # Islak
    toprak["islak"] = fuzz.trapmf(
        toprak.universe,
        [
            max_toprak,
            max_toprak,
            min(100, max_toprak + toprak_aralik),
            100
        ]
    )
    #aynı işlemleri ortam nemi için de yapıyorum yine max 100 ve 0 aralığında olacak bunlar değiştiricez
    ortam_nem_fuzzy = ctrl.Antecedent(
        np.arange(0, 101, 1),
        "ortam_nem"
    )

    ortam_aralik = max_ortam_nemi - min_ortam_nemi

    # Düşük
    ortam_nem_fuzzy["dusuk"] = fuzz.trapmf(
        ortam_nem_fuzzy.universe,
        [
            0,
            0,
            max(0, min_ortam_nemi - ortam_aralik),
            min_ortam_nemi
        ]
    )

    # İdeal
    ortam_nem_fuzzy["ideal"] = fuzz.trapmf(
        ortam_nem_fuzzy.universe,
        [
            min_ortam_nemi,
            min_ortam_nemi,
            max_ortam_nemi,
            max_ortam_nemi
        ]
    )

    # Yüksek
    ortam_nem_fuzzy["yuksek"] = fuzz.trapmf(
        ortam_nem_fuzzy.universe,
        [
            max_ortam_nemi,
            max_ortam_nemi,
            min(100, max_ortam_nemi + ortam_aralik),
            100
        ]
    )

    #şimdi de  sıcaklık için yapıcam ısı aralığı yine değişebilir olacak şimdilil bu şekilde yapıyorum ve üyelik fonksiyonu DÜŞÜK İDEAL YÜKSEK şeklinde:
    sicaklik_fuzzy = ctrl.Antecedent(
        np.arange(0, 51, 1),
        "sicaklik"
    )

    sicaklik_aralik = max_sicaklik - min_sicaklik

    # Düşük
    sicaklik_fuzzy["dusuk"] = fuzz.trapmf(
        sicaklik_fuzzy.universe,
        [
            0,
            0,
            max(0, min_sicaklik - sicaklik_aralik),
            min_sicaklik
        ]
    )

    # İdeal
    sicaklik_fuzzy["ideal"] = fuzz.trapmf(
        sicaklik_fuzzy.universe,
        [
            min_sicaklik,
            min_sicaklik,
            max_sicaklik,
            max_sicaklik
        ]
    )

    # Yüksek
    sicaklik_fuzzy["yuksek"] = fuzz.trapmf(
        sicaklik_fuzzy.universe,
        [
            max_sicaklik,
            max_sicaklik,
            min(50, max_sicaklik + sicaklik_aralik),
            50
        ]
    )
    #SULAMA kısmı için üyelik fonksiyonlarını yazdım şimdi karar kısmı için sonuç bölümünü yazıcam ve ÇOK FAZLA-FAZLA-ORTA-AZ-YOK şeklinde sulama yapılma değerleri olacak
    #öncelikle saniye olarak yaptım ve 30 sn ile kısıtladım değiştiriebilir kısım:
    # ============================================================
    # 11. SULAMA ÇIKTI ÜYELİK FONKSİYONLARI
    # ============================================================

    SULAMA_MAX_SURE = 30 #bu değişebilir

    sulama = ctrl.Consequent(
        np.arange(0, SULAMA_MAX_SURE + 1, 1),
        "sulama"
    )

    # Sulama yok
    sulama["yok"] = fuzz.trimf(
        sulama.universe,
        [
            0,
            0,
            SULAMA_MAX_SURE * 0.10
        ]
    )

    # Az sulama
    sulama["az"] = fuzz.trimf(
        sulama.universe,
        [
            0,
            SULAMA_MAX_SURE * 0.25,
            SULAMA_MAX_SURE * 0.50
        ]
    )

    # Orta sulama
    sulama["orta"] = fuzz.trimf(
        sulama.universe,
        [
            SULAMA_MAX_SURE * 0.25,
            SULAMA_MAX_SURE * 0.50,
            SULAMA_MAX_SURE * 0.75
        ]
    )

    # Fazla sulama
    sulama["fazla"] = fuzz.trimf(
        sulama.universe,
        [
            SULAMA_MAX_SURE * 0.50,
            SULAMA_MAX_SURE * 0.75,
            SULAMA_MAX_SURE
        ]
    )

    # Çok fazla sulama
    sulama["cok_fazla"] = fuzz.trimf(
        sulama.universe,
        [
            SULAMA_MAX_SURE * 0.75,
            SULAMA_MAX_SURE,
            SULAMA_MAX_SURE
        ]
    )
    #bu kısımda yine parametreler kullanabilirim sorarım yine.
    # işte şimdi bu üyelik fonksiyonlarına göre 4*3*3=36 kuralın hepsini yazıcam ya hak:
    rule1 = ctrl.Rule(
        toprak["cok_kuru"] & sicaklik_fuzzy["dusuk"] & ortam_nem_fuzzy["dusuk"],
        sulama["fazla"]
    )

    rule2 = ctrl.Rule(
        toprak["cok_kuru"] & sicaklik_fuzzy["dusuk"] & ortam_nem_fuzzy["ideal"],
        sulama["fazla"]
    )

    rule3 = ctrl.Rule(
        toprak["cok_kuru"] & sicaklik_fuzzy["dusuk"] & ortam_nem_fuzzy["yuksek"],
        sulama["orta"]
    )
    rule4 = ctrl.Rule(
        toprak["cok_kuru"] & sicaklik_fuzzy["ideal"] & ortam_nem_fuzzy["dusuk"],
        sulama["cok_fazla"]
    )
    rule5 = ctrl.Rule(
        toprak["cok_kuru"] & sicaklik_fuzzy["ideal"] & ortam_nem_fuzzy["ideal"],
        sulama["fazla"]
    )
    rule6 = ctrl.Rule(
        toprak["cok_kuru"] & sicaklik_fuzzy["ideal"] & ortam_nem_fuzzy["yuksek"],
        sulama["orta"]
    )
    rule7 = ctrl.Rule(
        toprak["cok_kuru"] & sicaklik_fuzzy["yuksek"] & ortam_nem_fuzzy["dusuk"],
        sulama["cok_fazla"]
    )
    rule8 = ctrl.Rule(
        toprak["cok_kuru"] & sicaklik_fuzzy["yuksek"] & ortam_nem_fuzzy["ideal"],
        sulama["cok_fazla"]
    )
    rule9 = ctrl.Rule(
        toprak["cok_kuru"] & sicaklik_fuzzy["yuksek"] & ortam_nem_fuzzy["yuksek"],
        sulama["fazla"]
    )
    #şimdi toprak nemi KURU olan 9 tane kurala geçtim:
    rule10 = ctrl.Rule(
        toprak["kuru"] & sicaklik_fuzzy["dusuk"] & ortam_nem_fuzzy["dusuk"],
        sulama["orta"]
    )
    rule11 = ctrl.Rule(
        toprak["kuru"] & sicaklik_fuzzy["dusuk"] & ortam_nem_fuzzy["ideal"],
        sulama["orta"]
    )
    rule12 = ctrl.Rule(
        toprak["kuru"] & sicaklik_fuzzy["dusuk"] & ortam_nem_fuzzy["yuksek"],
        sulama["az"]
    )
    rule13 = ctrl.Rule(
        toprak["kuru"] & sicaklik_fuzzy["ideal"] & ortam_nem_fuzzy["dusuk"],
        sulama["fazla"]
    )
    rule14 = ctrl.Rule(
        toprak["kuru"] & sicaklik_fuzzy["ideal"] & ortam_nem_fuzzy["ideal"],
        sulama["az"]
    )

    rule15 = ctrl.Rule(
        toprak["kuru"] & sicaklik_fuzzy["ideal"] & ortam_nem_fuzzy["yuksek"],
        sulama["az"]
    )
    rule16 = ctrl.Rule(
        toprak["kuru"] & sicaklik_fuzzy["yuksek"] & ortam_nem_fuzzy["dusuk"],
        sulama["fazla"]
    )

    rule17 = ctrl.Rule(
        toprak["kuru"] & sicaklik_fuzzy["yuksek"] & ortam_nem_fuzzy["ideal"],
        sulama["orta"]
    )

    rule18 = ctrl.Rule(
        toprak["kuru"] &  sicaklik_fuzzy["yuksek"] & ortam_nem_fuzzy["yuksek"],
        sulama["orta"]
    )
    rule19 = ctrl.Rule(
        toprak["ideal"] & sicaklik_fuzzy["dusuk"] & ortam_nem_fuzzy["dusuk"],
        sulama["yok"]
    )

    rule20 = ctrl.Rule(
        toprak["ideal"] & sicaklik_fuzzy["dusuk"] & ortam_nem_fuzzy["ideal"],
        sulama["yok"]
    )

    rule21 = ctrl.Rule(
        toprak["ideal"] & sicaklik_fuzzy["dusuk"] & ortam_nem_fuzzy["yuksek"],
        sulama["yok"]
    )

    rule22 = ctrl.Rule(
        toprak["ideal"] & sicaklik_fuzzy["ideal"] &  ortam_nem_fuzzy["dusuk"],
        sulama["yok"]
    )

    rule23 = ctrl.Rule(
        toprak["ideal"] &  sicaklik_fuzzy["ideal"] & ortam_nem_fuzzy["ideal"],
        sulama["yok"]
    )

    rule24 = ctrl.Rule(
        toprak["ideal"] & sicaklik_fuzzy["ideal"] &  ortam_nem_fuzzy["yuksek"],
        sulama["yok"]
    )

    rule25 = ctrl.Rule(
        toprak["ideal"] &  sicaklik_fuzzy["yuksek"] &  ortam_nem_fuzzy["dusuk"],
        sulama["yok"]
    )

    rule26 = ctrl.Rule(
        toprak["ideal"] & sicaklik_fuzzy["yuksek"] & ortam_nem_fuzzy["ideal"],
        sulama["yok"]
    )

    rule27 = ctrl.Rule(
        toprak["ideal"] & sicaklik_fuzzy["yuksek"] & ortam_nem_fuzzy["yuksek"],
        sulama["yok"]
    )
    rule28 = ctrl.Rule(
        toprak["islak"] & sicaklik_fuzzy["dusuk"] & ortam_nem_fuzzy["dusuk"],
        sulama["yok"]
    )

    rule29 = ctrl.Rule(
        toprak["islak"] & sicaklik_fuzzy["dusuk"] & ortam_nem_fuzzy["ideal"],
        sulama["yok"]
    )

    rule30 = ctrl.Rule(
        toprak["islak"] &  sicaklik_fuzzy["dusuk"] & ortam_nem_fuzzy["yuksek"],
        sulama["yok"]
    )

    rule31 = ctrl.Rule(
        toprak["islak"] & sicaklik_fuzzy["ideal"] & ortam_nem_fuzzy["dusuk"],
        sulama["yok"]
    )

    rule32 = ctrl.Rule(
        toprak["islak"] & sicaklik_fuzzy["ideal"] & ortam_nem_fuzzy["ideal"],
        sulama["yok"]
    )

    rule33 = ctrl.Rule(
        toprak["islak"] & sicaklik_fuzzy["ideal"] & ortam_nem_fuzzy["yuksek"],
        sulama["yok"]
    )

    rule34 = ctrl.Rule(
        toprak["islak"] &  sicaklik_fuzzy["yuksek"] & ortam_nem_fuzzy["dusuk"],
        sulama["yok"]
    )

    rule35 = ctrl.Rule(
        toprak["islak"] & sicaklik_fuzzy["yuksek"] & ortam_nem_fuzzy["ideal"],
        sulama["yok"]
    )

    rule36 = ctrl.Rule(
        toprak["islak"] &  sicaklik_fuzzy["yuksek"] & ortam_nem_fuzzy["yuksek"],
        sulama["yok"]
    )#şükrolsun
    #şimdi 36 ayrı kuralı tek kontrl sistemine getirmek için birleştiriyorum:
    sulama_control = ctrl.ControlSystem([
        rule1,
        rule2,
        rule3,
        rule4,
        rule5,
        rule6,
        rule7,
        rule8,
        rule9,

        rule10,
        rule11,
        rule12,
        rule13,
        rule14,
        rule15,
        rule16,
        rule17,
        rule18,

        rule19,
        rule20,
        rule21,
        rule22,
        rule23,
        rule24,
        rule25,
        rule26,
        rule27,

        rule28,
        rule29,
        rule30,
        rule31,
        rule32,
        rule33,
        rule34,
        rule35,
        rule36
    ])
    #şimdi tüm kural sşstemi içinden anlık verilerle çalışacak şekle getriiyorum:
    sulama_sim = ctrl.ControlSystemSimulation(
        sulama_control
    )#aşağıda bu 3 değeri jsondan çıkarıyoruz işlem için:
    sulama_sim.input["toprak"] = toprak_nemi

    sulama_sim.input["sicaklik"] = sicaklik

    sulama_sim.input["ortam_nem"] = ortam_nemi#burada skfuzzy json nedir bilmediği için alınan değişkenleri yerine koyuyorum:

    sulama_sim.compute() #bullanık mantık hesaplaması burada gerçekleşiyor
    sulama_suresi = sulama_sim.output["sulama"] #sonucu ise burada alıyorum umarım çalışır gözlerim ağrıdı.

    #2.YANİ HAVALANDIRMA İÇİN İŞLEMLER
    #yine aynı şekilde önce değiştirilebilecek geçici sınırlar sonra kurallar yazılacak
    FAN_MAX_SEVIYE = 100

    fan = ctrl.Consequent(
        np.arange(0, FAN_MAX_SEVIYE + 1, 1),
        "fan"
    )

    # KAPALI

    fan["kapali"] = fuzz.trimf(
        fan.universe,
        [
            0,
            0,
            FAN_MAX_SEVIYE * 0.20
        ]
    )

    # DÜŞÜK

    fan["dusuk"] = fuzz.trimf(
        fan.universe,
        [
            FAN_MAX_SEVIYE * 0.10,
            FAN_MAX_SEVIYE * 0.30,
            FAN_MAX_SEVIYE * 0.50
        ]
    )

    # ORTA

    fan["orta"] = fuzz.trimf(
        fan.universe,
        [
            FAN_MAX_SEVIYE * 0.30,
            FAN_MAX_SEVIYE * 0.50,
            FAN_MAX_SEVIYE * 0.70
        ]
    )

    # YÜKSEK

    fan["yuksek"] = fuzz.trimf(
        fan.universe,
        [
            FAN_MAX_SEVIYE * 0.50,
            FAN_MAX_SEVIYE * 0.70,
            FAN_MAX_SEVIYE * 0.90
        ]
    )

    # ÇOK YÜKSEK
    fan["cok_yuksek"] = fuzz.trimf(
        fan.universe,
        [
            FAN_MAX_SEVIYE * 0.80,
            FAN_MAX_SEVIYE,
            FAN_MAX_SEVIYE
        ]
    )

    #çıktılarımız tamamlandı şimdi sıra kurallarda neyseki 3*3=9 kural.
    fan_rule1 = ctrl.Rule(
        sicaklik_fuzzy["dusuk"] & ortam_nem_fuzzy["dusuk"],
        fan["kapali"]
    )
    fan_rule2 = ctrl.Rule(
        sicaklik_fuzzy["dusuk"] & ortam_nem_fuzzy["ideal"],
        fan["kapali"]
    )
    fan_rule3 = ctrl.Rule(
        sicaklik_fuzzy["dusuk"] & ortam_nem_fuzzy["yuksek"],
        fan["dusuk"]
    )
    fan_rule4 = ctrl.Rule(
        sicaklik_fuzzy["ideal"] & ortam_nem_fuzzy["dusuk"],
        fan["dusuk"]
    )
    fan_rule5 = ctrl.Rule(
        sicaklik_fuzzy["ideal"] & ortam_nem_fuzzy["ideal"],
        fan["dusuk"]
    )
    fan_rule6 = ctrl.Rule(
        sicaklik_fuzzy["ideal"] & ortam_nem_fuzzy["yuksek"],
        fan["orta"]
    )
    fan_rule7 = ctrl.Rule(
        sicaklik_fuzzy["yuksek"] & ortam_nem_fuzzy["dusuk"],
        fan["orta"]
    )
    fan_rule8 = ctrl.Rule(
        sicaklik_fuzzy["yuksek"] & ortam_nem_fuzzy["ideal"],
        fan["yuksek"]
    )
    fan_rule9 = ctrl.Rule(
        sicaklik_fuzzy["yuksek"] & ortam_nem_fuzzy["yuksek"],
        fan["cok_yuksek"]
    )
    #şimdi bu 9 kuralı tek bir fuzzy sistemde birleştiriyorum:
    havalandirma_control = ctrl.ControlSystem([
        fan_rule1,
        fan_rule2,
        fan_rule3,
        fan_rule4,
        fan_rule5,
        fan_rule6,
        fan_rule7,
        fan_rule8,
        fan_rule9
    ])#burada fan çıktı üyelikleri haline geliyor.
    #sistemde çalıştırabilmek için.
    havalandirma_sim = ctrl.ControlSystemSimulation(
        havalandirma_control
    )
    #şimdi de bu jsondaki değerleri havalandırma fuzzy sistemi için canlı değerler haline getirmeliyiz yani verileri canlandırcam.
    havalandirma_sim.input["sicaklik"] = sicaklik #JSON'dan gelen sıcaklık değerini havalandırma fuzzy sisteminin sıcaklık girdisine bağla anlamına geliyor.
    havalandirma_sim.input["ortam_nem"] = ortam_nemi
    #fuzzy hesabı yaptırcam:
    havalandirma_sim.compute()
    #fuzzy sonucunu yazdırıyorum:
    fan_seviyesi = havalandirma_sim.output["fan"]
    #2. kısmı da yazdım şimdi 3. kısımda
    #3. kısım ISITMA
    isitma_karar = ctrl.Consequent(
        np.arange(0, 3.01, 0.01),
        "isitma_karar"
    )

    # ISIYI DÜŞÜR
    # Isıyı düşür
    isitma_karar["isi_dusur"] = fuzz.trimf(
        isitma_karar.universe,
        [0, 0, 1]
    )

    # Sabit tut
    isitma_karar["sabit_tut"] = fuzz.trimf(
        isitma_karar.universe,
        [0, 1, 2]
    )

    # Isıyı yükselt
    isitma_karar["isi_yukselt"] = fuzz.trimf(
        isitma_karar.universe,
        [1, 2, 2]
    )

    # 22. ISITMA KURALLARI
    # Sıcaklık düşükse hedef sıcaklığı yükselt
    # Sıcaklık düşükse ısıyı yükselt
    # Sıcaklık düşükse → ısıyı yükselt
    isitma_rule1 = ctrl.Rule(
        sicaklik_fuzzy["dusuk"],
        isitma_karar["isi_yukselt"]
    )

    # Sıcaklık ideal ise → sabit tut
    isitma_rule2 = ctrl.Rule(
        sicaklik_fuzzy["ideal"],
        isitma_karar["sabit_tut"]
    )

    # Sıcaklık yüksekse → ısıyı düşür
    isitma_rule3 = ctrl.Rule(
        sicaklik_fuzzy["yuksek"],
        isitma_karar["isi_dusur"]
    )
    #bu 3 kuralı tek bir kontrol sisteminde birleştiriyorum.
    isitma_control = ctrl.ControlSystem([
        isitma_rule1,
        isitma_rule2,
        isitma_rule3
    ])
    #bu sistemi çalıştırmak için simüle ediyoum.
    isitma_sim = ctrl.ControlSystemSimulation(
        isitma_control
    )

    isitma_sim.input["sicaklik"] = sicaklik

    isitma_sim.compute()

    hedef_sicaklik = isitma_sim.output["isitma_karar"]
    # şimdi tüm kararları birleştiricem önce sulama kararları için:
    if sulama_suresi < 5:
        sulama_karari = "sulama yok"

    elif sulama_suresi < 15:
        sulama_karari = "az sulama"

    elif sulama_suresi < 22:
        sulama_karari = "orta sulama"

    elif sulama_suresi < 27:
        sulama_karari = "fazla sulama"

    else:
        sulama_karari = "cok fazla sulama"
    #şimdi havalandırma kararları:
    if fan_seviyesi < 10:
        fan_karari = "fan kapali"

    elif fan_seviyesi < 30:
        fan_karari = "dusuk fan"

    elif fan_seviyesi < 60:
        fan_karari = "orta fan"

    elif fan_seviyesi < 80:
        fan_karari = "yuksek fan"

    else:
        fan_karari = "cok yuksek fan"
    #ve ısıtma kararları:
    isitma_fuzzy_sonucu = isitma_sim.output["isitma_karar"]
    if isitma_fuzzy_sonucu < 0.5:

        isitma_karari = "isi_dusur"

    elif isitma_fuzzy_sonucu < 1.5:

        isitma_karari = "sabit_tut"

    else:

        isitma_karari = "isi_yukselt"

    if isitma_karari == "isi_yukselt":

        # Sıcaklık optimal değerin altında.
        # Sistemin hedefi optimal maksimum sıcaklık.
        hedef_sicaklik = max_sicaklik

    elif isitma_karari == "sabit_tut":

        # Sıcaklık optimal aralıkta.
        # Mevcut sıcaklık korunacak.
        hedef_sicaklik = sicaklik

    else:

        # Sıcaklık optimal değerin üzerinde.
        # Hedef optimal minimum sıcaklık.
        hedef_sicaklik = min_sicaklik

    #burada fuzzy sistemin ürettiği sayısal değer durumlarını backendin anlayabileceği sözel durumlra çevirdim.
    #şimdi backende gönderilecek json yazdırcam.
    karar_json = {

        "bitki": bitki["bitkiAdi"],

        "evre": bitki["evreAdi"],

        "kararlar": {

            "sulama": {
                "sure": round(float(sulama_suresi), 2),
                "karar": sulama_karari
            },

            "havalandirma": {
                "seviye": round(float(fan_seviyesi), 2),
                "karar": fan_karari
            },

            "isitma": {
                "seviye": round(float(hedef_sicaklik), 2),
                "karar": isitma_karari
            }
        }
    }

    return karar_json


# ============================================================
# Flask HTTP API - Backend bu endpoint'e POST atar
# ============================================================
from flask import Flask, request, jsonify

app = Flask(__name__)

@app.route("/api/fuzzy/calculate", methods=["POST"])
def calculate():
    json_veri = request.get_json()
    if not json_veri:
        return jsonify({"hata": "JSON verisi bekleniyor"}), 400
    try:
        sonuc = hesapla(json_veri)
        return jsonify(sonuc)
    except Exception as e:
        return jsonify({"hata": str(e)}), 500


if __name__ == "__main__":
    # test etmek için ekrana yazdır
    sonuc = hesapla(test_json)
    json_cikti = json.dumps(
        sonuc,
        ensure_ascii=False,
        indent=4
    )
    # test etmek için ekrana yazdır
    print(json_cikti)

    # Flask sunucusunu baslat
    app.run(host="0.0.0.0", port=5000)
