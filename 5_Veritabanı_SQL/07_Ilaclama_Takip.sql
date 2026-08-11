create table Ilaclama_Takip (
	ilaclama_ID int primary key identity(1,1),
	hastalik_ID int foreign key references Bitki_Hastalik(hastalik_ID),
	ilacAdi nvarchar(100),
	uygulamaZamani datetime default getdate()
);

select * from Ilaclama_Takip;