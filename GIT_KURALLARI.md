# 🌿 Akıllı Sera Projesi - Git ve GitHub Kullanım Rehberi

Aramıza hoş geldin! 🎉 Projemizde kodların silinmemesi, kimsenin emeğinin boşa gitmemesi ve sistemin çökmemesi için **Git** kullanıyoruz. 

Eğer Git'i ilk defa kullanıyorsan hiç endişelenme. Bu rehber sana adım adım ne yapman gerektiğini en basit haliyle anlatacak. ✌️

---

## 🚫 Altın Kural: `main` Dalına Dokunmak Yasaktır!

`main` (ana) dalı, projemizin **vitrinidir**. Müşteriye veya hocalarımıza sunacağımız, çalışan, kusursuz kodların bulunduğu yerdir. 

> 💡 **Neden Yasak?** 
> Eğer herkes aynı anda vitrindeki motoru söküp takmaya çalışırsa sistem çöker, kodlar birbirine girer. Bunun yerine, vitrindeki projenin bir kopyasını alıp kendi **arka atölyemize (branch)** götüreceğiz. İşimizi orada bitirip test ettikten sonra "Bunu vitrine alalım mı?" diye ekibe soracağız.

---

## 🛡️ Gizli Kahraman: `.gitignore` Nedir?

`.gitignore` dosyası, projemizin **kapıdaki güvenlik görevlisidir**. 

Sen terminalde kodları paketlemek için `git add .` komutunu yazdığında, bu dosya devreye girer ve klasöründeki gereksiz veya devasa dosyaların GitHub'a gitmesini engeller.
*   🤖 **Yapay Zeka Ekibi:** Dev `.onnx` veya `.pt` model dosyalarının yüklenmesi engellenir.
*   ⚙️ **Backend / Gömülü Ekibi:** Derleme sırasında otomatik oluşan çöpler (`bin/`, `obj/`, `.pio/`) engellenir.
*   💻 **Frontend Ekibi:** Yüzlerce megabaytlık `node_modules/` klasörü engellenir.

> 🛑 **Senin bir şey yapmana gerek yok!** Biz bu güvenlik kurallarını her klasörün içine baştan ekledik. Eğer klasörüne koyduğun büyük bir model dosyası veya test verisi GitHub'a yüklenmezse panik yapma; güvenlik görevlimiz işini yapıyor demektir!

---

## 🛠️ Günlük Çalışma Rutinimiz (3 Basit Adım)

Her gün bilgisayar başına oturduğunda şu 3 adımı sırasıyla yapmalısın:

### 🟢 ADIM 1: Güne Başlarken (Atölyeni Hazırla)
Dün gece takım arkadaşların projeye yeni kodlar eklemiş olabilir. Önce o yeni kodları bilgisayarına çekmeli, sonra kendine ait boş bir çalışma masası (dal/branch) açmalısın. Terminaline şunları yaz:

`git checkout main`
`git pull`

*(Kendine yeni bir çalışma odası aç ve oraya geç. Örnek: feature/SCRUM-9-ai-modeli)*
`git checkout -b feature/gorev-adiniz`

*✨ Harika! Artık kendi güvenli odandasın. Burada neyi bozarsan boz, ana projeye hiçbir şey olmaz.*

---

### 🟡 ADIM 2: Çalışırken (Kodlarını Paketle)
Kendi klasöründe kodlarını yazdın, denedin ve her şey süper çalışıyor. Şimdi bu yazdığın yeni kodları GitHub'a kargolamak için paketlemeliyiz.

*(Değiştirdiğin tüm dosyaları kargo paketinin içine koy)*
`git add .`

*(Paketin üzerine ne yaptığını yazan kısa bir etiket yapıştır)*
`git commit -m "Domates hastalık tespit modeli eklendi ve test edildi"`

> ⚠️ **Önemli İpucu:** Lütfen `"asdf"`, `"kod güncellendi"` gibi anlamsız yazılar yazma! Ekip arkadaşların binlerce satır kodu okumadan önce bu mesaja bakarak senin ne yaptığını şıp diye anlamalı.

---

### 🔴 ADIM 3: İşi Bitirirken (GitHub'a Gönder)
Paketimiz hazır. Şimdi bu paketi bilgisayarından GitHub'a (kendi odana) gönderme vakti.

*(Hazırladığın paketi GitHub'a fırlat)*
`git push origin feature/gorev-adiniz`

---

## ✅ Son Aşama: Onay İstemek (Pull Request)

Terminaldeki işimiz bitti! Kodunu GitHub'a gönderdin ama kodun henüz `main` (vitrin) kısmına geçmedi. Bunun için ekipten onay istemelisin. Bu işleme **Pull Request (PR)** diyoruz.

1. **GitHub web sitesine** gir ve projemizin sayfasına bak.
2. Ekranın üst kısmında yeşil renkli **"Compare & pull request"** butonunu göreceksin. Ona tıkla.
3. Ekibe ne yaptığını anlatan ufak bir açıklama yaz ve **"Create pull request"** butonuna bas.

🎉 **Bitti!** Artık takım arkadaşların kodunu inceleyecek. Eğer bir sorun yoksa yeşil **Merge (Birleştir)** butonuna basacaklar ve senin kodun ana projeye resmen dahil olacak! Ellerinize sağlık.

> 💬 **Unutma:** Hata yapmaktan asla korkma. Git sisteminde silinen veya bozulan her şeyin bir geri dönüşü (undo) vardır. Takıldığın veya emin olamadığın her yerde mutlaka ekipten yardım iste!