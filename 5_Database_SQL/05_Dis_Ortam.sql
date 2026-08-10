create table Dis_Ortam_Loglari (
	Log_ID int primary key identity(1,1),
	Sera_ID int foreign key references Sera_Durum(Sera_ID),
	HavaDurumu nvarchar(50),
	DisOrtamSicakligi decimal (4,2),
	DisOrtamNemi decimal (4,2),
	KayitZamani datetime default getdate()
);

select * from Dis_Ortam_Loglari;