-- bağımsız tablolar oluştruma:

create table Bitki_Evreleri(
	evre_ID int primary key identity(1,1),
	bitkiAdi nvarchar(50),
	evreAdi nvarchar(50),
	minSicaklik decimal(4,2),
	maxSicaklik decimal(4,2),
	minToprakNemi decimal(4,2),
	maxToprakNemi decimal(4,2),
	minOrtamNemi decimal(4,2),
	maxOrtamNemi decimal(4,2)
);

create table Sera_Durum (
	sera_ID int primary key identity(1,1),
	seraAdi nvarchar(100),
	AktifEvreID int foreign key references Bitki_Evreleri(evre_ID),  -- bitkievreleri tablosunun evreıd değerleri gelecek hep, bağladık birbirine !
	SonGuncellemeZamani datetime
);

alter table Bitki_Evreleri drop column maxSicaklik;

select * from Bitki_Evreleri;

alter table Bitki_Evreleri
add
	gunduzMinSicaklik decimal(4,2),
    gunduzMaxSicaklik decimal(4,2),
    geceMinSicaklik decimal(4,2),
    geceMaxSicaklik decimal(4,2);

alter table Sera_Durum
add
	koordinatEylem decimal(8,6), -- dünya enlemleri -90.000000 ile +90.000000 arasında değişir 
	koordinatBoylam decimal(9,6) -- dünya boylamları -180.000000 ile +90.000000 arasında değişir

EXEC sp_rename 'Sera_Durum.koordinatEylem', 'koordinatEnlem', 'COLUMN';   -- sp_rename sütun adı değiştirir !!!

alter table Bitki_Evreleri alter column minToprakNemi decimal(5,2);
alter table Bitki_Evreleri alter column maxToprakNemi decimal(5,2);
alter table Bitki_Evreleri alter column minOrtamNemi decimal(5,2);
alter table Bitki_Evreleri alter column maxOrtamNemi decimal(5,2);
alter table Bitki_Evreleri alter column gunduzMinSicaklik decimal(5,2);
alter table Bitki_Evreleri alter column gunduzMaxSicaklik decimal(5,2);
alter table Bitki_Evreleri alter column geceMinSicaklik decimal(5,2);
alter table Bitki_Evreleri alter column geceMaxSicaklik decimal(5,2);

SELECT 
    s.sera_ID, 
    s.seraAdi,  
    s.AktifEvreID, 
    e.evreAdi
FROM Sera_Durum s
JOIN Bitki_Evreleri e ON s.AktifEvreID = e.evre_ID;