create procedure sp_SensorVerisiEkle 
	@Sera_ID int,
	@OrtamSicakligi decimal(4,2),
	@OrtamNemi decimal(4,2),
	@ToprakNemi decimal(4,2)
as
begin
	insert into Sensor_Loglari (Sera_ID, OrtamSicakligi, OrtamNemi, ToprakNemi)
	values (@Sera_ID, @OrtamSicakligi, @OrtamNemi, @ToprakNemi);
end;

go

create procedure sp_KameraVerisiEkle
	@Sera_ID int,
	@DurumBilgisi nvarchar(200)
as
begin
	insert into Kamera_Loglari (Sera_ID, DurumBilgisi)
	values (@Sera_ID, @DurumBilgisi);
end;

go

create procedure sp_AksiyonVerisiEkle
	@Sera_ID int,
	@CihazAdi nvarchar(50),
	@Aksiyon nvarchar(100)
as
begin
	insert into Aksiyon_Loglari (Sera_ID, CihazAdi, Aksiyon)
	values (@Sera_ID, @CihazAdi, @Aksiyon);
end;

go

create procedure sp_DisOrtamVerisiEkle
	@Sera_ID int,
	@HavaDurumu nvarchar(50),
	@DisOrtamSicakligi decimal (4,2),
	@DisOrtamNemi decimal (4,2)
as
begin
	insert into Dis_Ortam_Loglari (Sera_ID, HavaDurumu, DisOrtamSicakligi, DisOrtamNemi)
	values (@Sera_ID, @HavaDurumu, @DisOrtamSicakligi, @DisOrtamNemi);
end;

go

create procedure sp_SeraEvreGuncelle
	@Sera_ID int,
	@YeniEvre_ID int
as
begin
	update Sera_Durum    -- update çünkü güncelleme yapıuoruz diğerleri tabloya ekleme yapmaktı !!!
	set AktifEvreID = @YeniEvre_ID where sera_ID = @Sera_ID;
end;

go

create procedure sp_SeraIdealDegerleriGetir -- karşılaştırma yapabilmek için !!!
	@Sera_ID int
as
begin
	select 
		Sera_Durum.sera_ID,
		Sera_Durum.seraAdi,
		Bitki_Evreleri.evreAdi,
		Bitki_Evreleri.minToprakNemi,
		Bitki_Evreleri.maxToprakNemi,
		Bitki_Evreleri.minOrtamNemi,
		Bitki_Evreleri.maxOrtamNemi,
		Bitki_Evreleri.gunduzMinSicaklik,
		Bitki_Evreleri.gunduzMaxSicaklik,
		Bitki_Evreleri.geceMinSicaklik,
		Bitki_Evreleri.geceMaxSicaklik
	from Sera_Durum
	inner join Bitki_Evreleri on Sera_Durum.AktifEvreID = Bitki_Evreleri.evre_ID
	where Sera_Durum.sera_ID = @Sera_ID; 
end;

go

create procedure sp_KullaniciKayit
	@isim nvarchar(20),
	@soyisim nvarchar(20),
	@eposta nvarchar(50),
	@sifre nvarchar(10)
as
begin
	insert into Kullanicilar (isim, soyisim, eposta, sifre)
	values (@isim, @soyisim, @eposta, @sifre);
end;

go

create procedure sp_KullaniciGirisKontrol
	@eposta nvarchar(50),
	@sifre nvarchar(10)
as
begin
	select 
		Kullanicilar.eposta,
		Kullanicilar.sifre
	from Kullanicilar where Kullanicilar.eposta = @eposta and Kullanicilar.sifre = @sifre;
end;

go

create procedure sp_BitkiHastalikKayit 
	@bitki_ID int,
	@hastalikAdi nvarchar(50),
	@hastalikOrani decimal(4,2),
	@fotografYolu nvarchar(300)
as
begin
	insert into Bitki_Hastalik(bitki_ID, hastalikAdi, hastalikOrani, fotografYolu)
	values (@bitki_ID, @hastalikAdi, @hastalikOrani, @fotografYolu);
end;

go

create procedure sp_Bildirim 
	@mesaj nvarchar(200)
as
begin
	insert into Bildirim(mesaj)
	values (@mesaj);
end;

go

create procedure sp_IlaclamaYap
	@hastalik_ID int,
	@ilacAdi nvarchar(100),
	@mesaj nvarchar(200)
as
begin
	insert into Ilaclama_Takip(hastalik_ID, ilacAdi)
	values (@hastalik_ID, @ilacAdi);

	insert into Bildirim(mesaj)
	values(@mesaj);
end;