create table Kullanicilar (
	kullanici_ID int primary key identity(1,1),
	isim nvarchar(20),
	soyisim nvarchar(20),
	eposta nvarchar(50),
	sifre nvarchar(10)
);

select * from Kullanicilar;

create table Bitki_Hastalik (
	hastalik_ID int primary key identity(1,1),
	bitki_ID int foreign key references Sera_Durum(sera_ID),
	hastalikAdi nvarchar(50),
	hastalikOrani decimal(4,2),
	fotografYolu nvarchar(300)
);

select * from Bitki_Hastalik;

create table Bildirim (
	bildirim_ID int primary key identity(1,1),
	mesaj nvarchar(200),
	okunduBilgisi bit,
	BildirimZamani datetime default getdate()
);

select * from Bildirim;