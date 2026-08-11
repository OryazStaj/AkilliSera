using System;
using System.Collections.Generic;
using AkilliSera_API.Models;
using Microsoft.EntityFrameworkCore;

namespace AkilliSera_API.Data;

public partial class AkilliSeraDbContext : DbContext
{
    public AkilliSeraDbContext()
    {
    }

    public AkilliSeraDbContext(DbContextOptions<AkilliSeraDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AksiyonLoglari> AksiyonLoglaris { get; set; }

    public virtual DbSet<Bildirim> Bildirims { get; set; }

    public virtual DbSet<BitkiEvreleri> BitkiEvreleris { get; set; }

    public virtual DbSet<BitkiHastalik> BitkiHastaliks { get; set; }

    public virtual DbSet<DisOrtamLoglari> DisOrtamLoglaris { get; set; }

    public virtual DbSet<IlaclamaTakip> IlaclamaTakips { get; set; }

    public virtual DbSet<KameraLoglari> KameraLoglaris { get; set; }

    public virtual DbSet<Kullanicilar> Kullanicilars { get; set; }

    public virtual DbSet<SensorLoglari> SensorLoglaris { get; set; }

    public virtual DbSet<SeraDurum> SeraDurums { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=AkilliSeraDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AksiyonLoglari>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Aksiyon___2D26E7AEAC385006");

            entity.ToTable("Aksiyon_Loglari");

            entity.Property(e => e.LogId).HasColumnName("Log_ID");
            entity.Property(e => e.Aksiyon).HasMaxLength(100);
            entity.Property(e => e.CihazAdi).HasMaxLength(50);
            entity.Property(e => e.KayitZamani)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SeraId).HasColumnName("Sera_ID");

            entity.HasOne(d => d.Sera).WithMany(p => p.AksiyonLoglaris)
                .HasForeignKey(d => d.SeraId)
                .HasConstraintName("FK__Aksiyon_L__Sera___571DF1D5");
        });

        modelBuilder.Entity<Bildirim>(entity =>
        {
            entity.HasKey(e => e.BildirimId).HasName("PK__Bildirim__6F9E4B83ED79087B");

            entity.ToTable("Bildirim");

            entity.Property(e => e.BildirimId).HasColumnName("bildirim_ID");
            entity.Property(e => e.BildirimZamani)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Mesaj)
                .HasMaxLength(200)
                .HasColumnName("mesaj");
            entity.Property(e => e.OkunduBilgisi).HasColumnName("okunduBilgisi");
        });

        modelBuilder.Entity<BitkiEvreleri>(entity =>
        {
            entity.HasKey(e => e.EvreId).HasName("PK__Bitki_Ev__B11E4F1424ADE436");

            entity.ToTable("Bitki_Evreleri");

            entity.Property(e => e.EvreId).HasColumnName("evre_ID");
            entity.Property(e => e.BitkiAdi)
                .HasMaxLength(50)
                .HasColumnName("bitkiAdi");
            entity.Property(e => e.EvreAdi)
                .HasMaxLength(50)
                .HasColumnName("evreAdi");
            entity.Property(e => e.GeceMaxSicaklik)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("geceMaxSicaklik");
            entity.Property(e => e.GeceMinSicaklik)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("geceMinSicaklik");
            entity.Property(e => e.GunduzMaxSicaklik)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("gunduzMaxSicaklik");
            entity.Property(e => e.GunduzMinSicaklik)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("gunduzMinSicaklik");
            entity.Property(e => e.MaxOrtamNemi)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("maxOrtamNemi");
            entity.Property(e => e.MaxToprakNemi)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("maxToprakNemi");
            entity.Property(e => e.MinOrtamNemi)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("minOrtamNemi");
            entity.Property(e => e.MinSicaklik)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("minSicaklik");
            entity.Property(e => e.MinToprakNemi)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("minToprakNemi");
        });

        modelBuilder.Entity<BitkiHastalik>(entity =>
        {
            entity.HasKey(e => e.HastalikId).HasName("PK__Bitki_Ha__B165537B4B100B5D");

            entity.ToTable("Bitki_Hastalik");

            entity.Property(e => e.HastalikId).HasColumnName("hastalik_ID");
            entity.Property(e => e.BitkiId).HasColumnName("bitki_ID");
            entity.Property(e => e.FotografYolu)
                .HasMaxLength(300)
                .HasColumnName("fotografYolu");
            entity.Property(e => e.HastalikAdi)
                .HasMaxLength(50)
                .HasColumnName("hastalikAdi");
            entity.Property(e => e.HastalikOrani)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("hastalikOrani");

            entity.HasOne(d => d.Bitki).WithMany(p => p.BitkiHastaliks)
                .HasForeignKey(d => d.BitkiId)
                .HasConstraintName("FK__Bitki_Has__bitki__6A30C649");
        });

        modelBuilder.Entity<DisOrtamLoglari>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Dis_Orta__2D26E7AE22D235F9");

            entity.ToTable("Dis_Ortam_Loglari");

            entity.Property(e => e.LogId).HasColumnName("Log_ID");
            entity.Property(e => e.DisOrtamNemi).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.DisOrtamSicakligi).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.HavaDurumu).HasMaxLength(50);
            entity.Property(e => e.KayitZamani)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SeraId).HasColumnName("Sera_ID");

            entity.HasOne(d => d.Sera).WithMany(p => p.DisOrtamLoglaris)
                .HasForeignKey(d => d.SeraId)
                .HasConstraintName("FK__Dis_Ortam__Sera___6477ECF3");
        });

        modelBuilder.Entity<IlaclamaTakip>(entity =>
        {
            entity.HasKey(e => e.IlaclamaId).HasName("PK__Ilaclama__166BA761B02CBCC0");

            entity.ToTable("Ilaclama_Takip");

            entity.Property(e => e.IlaclamaId).HasColumnName("ilaclama_ID");
            entity.Property(e => e.HastalikId).HasColumnName("hastalik_ID");
            entity.Property(e => e.IlacAdi)
                .HasMaxLength(100)
                .HasColumnName("ilacAdi");
            entity.Property(e => e.UygulamaZamani)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("uygulamaZamani");

            entity.HasOne(d => d.Hastalik).WithMany(p => p.IlaclamaTakips)
                .HasForeignKey(d => d.HastalikId)
                .HasConstraintName("FK__Ilaclama___hasta__70DDC3D8");
        });

        modelBuilder.Entity<KameraLoglari>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Kamera_L__2D26E7AE7DA3378E");

            entity.ToTable("Kamera_Loglari");

            entity.Property(e => e.LogId).HasColumnName("Log_ID");
            entity.Property(e => e.DurumBilgisi).HasMaxLength(200);
            entity.Property(e => e.KayitZamani)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SeraId).HasColumnName("Sera_ID");

            entity.HasOne(d => d.Sera).WithMany(p => p.KameraLoglaris)
                .HasForeignKey(d => d.SeraId)
                .HasConstraintName("FK__Kamera_Lo__Sera___534D60F1");
        });

        modelBuilder.Entity<Kullanicilar>(entity =>
        {
            entity.HasKey(e => e.KullaniciId).HasName("PK__Kullanic__9F0FC71AD11B22A1");

            entity.ToTable("Kullanicilar");

            entity.Property(e => e.KullaniciId).HasColumnName("kullanici_ID");
            entity.Property(e => e.Eposta)
                .HasMaxLength(50)
                .HasColumnName("eposta");
            entity.Property(e => e.Isim)
                .HasMaxLength(20)
                .HasColumnName("isim");
            entity.Property(e => e.Sifre)
                .HasMaxLength(10)
                .HasColumnName("sifre");
            entity.Property(e => e.Soyisim)
                .HasMaxLength(20)
                .HasColumnName("soyisim");
        });

        modelBuilder.Entity<SensorLoglari>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Sensor_L__2D26E7AEB418DAA6");

            entity.ToTable("Sensor_Loglari");

            entity.Property(e => e.LogId).HasColumnName("Log_ID");
            entity.Property(e => e.KayitZamani)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrtamNemi).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.OrtamSicakligi).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.SeraId).HasColumnName("Sera_ID");
            entity.Property(e => e.ToprakNemi).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Sera).WithMany(p => p.SensorLoglaris)
                .HasForeignKey(d => d.SeraId)
                .HasConstraintName("FK__Sensor_Lo__Sera___4F7CD00D");
        });

        modelBuilder.Entity<SeraDurum>(entity =>
        {
            entity.HasKey(e => e.SeraId).HasName("PK__Sera_Dur__B058BFA0B84ABCDD");

            entity.ToTable("Sera_Durum");

            entity.Property(e => e.SeraId).HasColumnName("sera_ID");
            entity.Property(e => e.AktifEvreId).HasColumnName("AktifEvreID");
            entity.Property(e => e.KoordinatBoylam)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("koordinatBoylam");
            entity.Property(e => e.KoordinatEnlem)
                .HasColumnType("decimal(8, 6)")
                .HasColumnName("koordinatEnlem");
            entity.Property(e => e.SeraAdi)
                .HasMaxLength(100)
                .HasColumnName("seraAdi");
            entity.Property(e => e.SonGuncellemeZamani).HasColumnType("datetime");

            entity.HasOne(d => d.AktifEvre).WithMany(p => p.SeraDurums)
                .HasForeignKey(d => d.AktifEvreId)
                .HasConstraintName("FK__Sera_Duru__Aktif__4CA06362");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
