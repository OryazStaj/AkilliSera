INSERT INTO Bitki_Evreleri (bitkiAdi, evreAdi, minToprakNemi, maxToprakNemi, minOrtamNemi, maxOrtamNemi, gunduzMinSicaklik, gunduzMaxSicaklik, 
geceMinSicaklik, geceMaxSicaklik)
VALUES 
('Domates', 'Tohum', 70.00, 85.00, 75.00, 85.00, 24.00, 28.00, 20.00, 22.00),
('Domates', 'Fide', 60.00, 75.00, 65.00, 75.00, 21.00, 26.00, 16.00, 19.00),
('Domates', 'Olgunlasma', 50.00, 70.00, 50.00, 65.00, 22.00, 27.00, 15.00, 18.00);


INSERT INTO Sera_Durum (seraAdi, AktifEvreID, SonGuncellemeZamani)
VALUES 
('Ana Sera', 1, GETDATE());
select * from Bitki_Evreleri;
select * from Sera_Durum;


INSERT INTO Bitki_Evreleri (bitkiAdi, evreAdi, minToprakNemi, maxToprakNemi, minOrtamNemi, maxOrtamNemi, gunduzMinSicaklik, gunduzMaxSicaklik, 
geceMinSicaklik, geceMaxSicaklik)
VALUES 
('Patlıcan', 'Tohum', 70.00, 85.00, 75.00, 85.00, 25.00, 30.00, 20.00, 24.00),
('Patlıcan', 'Fide', 60.00, 75.00, 60.00, 70.00, 22.00, 28.00, 16.00, 20.00),
('Patlıcan', 'Olgunlasma', 50.00, 70.00, 50.00, 65.00, 22.00, 28.00, 16.00, 20.00);


INSERT INTO Sera_Durum (seraAdi, AktifEvreID, SonGuncellemeZamani)
VALUES 
('Sera-2', 2, GETDATE());

INSERT INTO Bitki_Evreleri (bitkiAdi, evreAdi, minToprakNemi, maxToprakNemi, minOrtamNemi, maxOrtamNemi, gunduzMinSicaklik, gunduzMaxSicaklik, 
geceMinSicaklik, geceMaxSicaklik)
VALUES 
('Biber', 'Tohum', 70.00, 85.00, 75.00, 85.00, 24.00, 28.00, 20.00, 22.00),
('Biber', 'Fide', 60.00, 75.00, 60.00, 70.00, 21.00, 26.00, 16.00, 19.00),
('Biber', 'Olgunlasma', 50.00, 70.00, 50.00, 65.00, 22.00, 27.00, 16.00, 19.00);

INSERT INTO Sera_Durum (seraAdi, AktifEvreID, SonGuncellemeZamani)
VALUES 
('Biberler Serası', 3, GETDATE());

update Sera_Durum set seraAdi = 'Domates Serası' where sera_ID = 1;
update Sera_Durum set seraAdi = 'Patlıcan Serası' where sera_ID = 2;