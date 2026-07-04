using System.ComponentModel.DataAnnotations;

namespace HalisahaApp.API.DTOs
{
    // Müsaitlik sorgusunda dönen saat dilimi
    public class SaatDilimiDto
    {
        public int BaslangicSaati { get; set; }
        public int BitisSaati { get; set; }
        public string Etiket { get; set; } = string.Empty; // "19:00 – 20:00"
        public bool Dolu { get; set; }
        public decimal Ucret { get; set; }
    }

    // Yeni rezervasyon oluşturma isteği
    public class RezervasyonOlusturDto
    {
        [Required(ErrorMessage = "Saha kodu zorunludur.")]
        public string SahaKodu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        [MaxLength(100)]
        public string AdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Geçerli bir telefon numarası girin.")]
        public string Telefon { get; set; } = string.Empty;

        [Required]
        public DateTime Tarih { get; set; }

        [Range(0, 23, ErrorMessage = "Geçersiz saat.")]
        public int BaslangicSaati { get; set; }
    }

    // iyzico ödeme callback
    public class OdemeCallbackDto
    {
        public int RezervasyonId { get; set; }
        public bool Basarili { get; set; }
        public string? IyzicoOdemeId { get; set; }
    }

    // Admin - Manuel rezervasyon ekleme
    public class ManuelRezervasyonDto
    {
        [Required]
        public int SahaId { get; set; }

        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        public string AdSoyad { get; set; } = string.Empty;

        public string? Email { get; set; }
        public string? Telefon { get; set; }

        [Required]
        public DateTime Tarih { get; set; }

        [Range(0, 23)]
        public int BaslangicSaati { get; set; }
    }

    // Admin - Rezervasyon iptal
    public class IptalDto
    {
        public string? Neden { get; set; }
    }

    // Admin - Saha ayarları güncelleme
    public class SahaAyarlariDto
    {
        [Required]
        public string Ad { get; set; } = string.Empty;

        [Range(0, 23)]
        public int AcilisSaati { get; set; } = 17;

        [Range(1, 24)]
        public int KapanisSaati { get; set; } = 24;

        [Range(0, 100000)]
        public decimal SaatlikUcret { get; set; }
    }

    // Admin - Giriş
    public class AdminGirisDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Sifre { get; set; } = string.Empty;
    }
    public class SahaEkleDto
    {
        [Required]
        public int AdminId { get; set; }
        [Required]
        public string Ad { get; set; } = string.Empty;
        [Range(0, 100000)]
        public decimal SaatlikUcret { get; set; } = 3000;
    }
}
