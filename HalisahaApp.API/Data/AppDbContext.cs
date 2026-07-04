using HalisahaApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HalisahaApp.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Admin> Adminler { get; set; }
        public DbSet<Saha> Sahalar { get; set; }
        public DbSet<Rezervasyon> Rezervasyonlar { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Saha
            modelBuilder.Entity<Saha>(entity =>
            {
                entity.HasIndex(s => s.SahaKodu).IsUnique();
                entity.Property(s => s.SaatlikUcret).HasPrecision(18, 2);
                entity.HasOne(s => s.Admin)
                      .WithMany(a => a.Sahalar)
                      .HasForeignKey(s => s.AdminId);
            });

            // Rezervasyon
            modelBuilder.Entity<Rezervasyon>(entity =>
            {
                entity.Property(r => r.ToplamUcret).HasPrecision(18, 2);

                // Aynı sahada aynı gün aynı saat çakışmasını önle (DB seviyesinde)
                entity.HasIndex(r => new { r.SahaId, r.Tarih, r.BaslangicSaati }).IsUnique();

                entity.HasOne(r => r.Saha)
                      .WithMany(s => s.Rezervasyonlar)
                      .HasForeignKey(r => r.SahaId);
            });

            // Seed Data - Örnek admin
            modelBuilder.Entity<Admin>().HasData(new Admin
            {
                Id = 1,
                AdSoyad = "Halısaha Yöneticisi",
                Email = "admin@halisaha.com",
                // Şifre: Admin123! (BCrypt hash)
                SifreHash = "$2a$11$example_hash_buraya_gelecek",
                OlusturmaTarihi = new DateTime(2025, 1, 1)
            });
        }
    }
}
