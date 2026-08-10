create table Sensor_Loglari (
	Log_ID int primary key identity(1,1),
	Sera_ID int foreign key references Sera_Durum(sera_ID),
	OrtamSicakligi decimal(4,2),
	OrtamNemi decimal(4,2),
	ToprakNemi decimal(4,2),
	KayitZamani datetime default getdate()
);

create table Kamera_Loglari(
	Log_ID int primary key identity(1,1),
	Sera_ID int foreign key references Sera_Durum(sera_ID),
	DurumBilgisi nvarchar(200),
	KayitZamani datetime default getdate()
);

create table Aksiyon_Loglari(
	Log_ID int primary key identity(1,1),
	Sera_ID int foreign key references Sera_Durum(sera_ID),
	CihazAdi nvarchar(50),
	Aksiyon nvarchar(100),
	KayitZamani datetime default getdate()
);


	

select * from Sensor_Loglari;
alter table Sensor_Loglari
add
	OrtamAydinligi bit;

alter table Sensor_Loglari
drop column OrtamAydinligi;
	
