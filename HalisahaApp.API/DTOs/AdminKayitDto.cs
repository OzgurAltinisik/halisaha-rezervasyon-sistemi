using System.ComponentModel.DataAnnotations;

namespace HalisahaApp.API.DTOs
{
    public class AdminKayitDto
    {
        [Required]
        public string AdSoyad { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Sifre { get; set; } = string.Empty;

        [Required]
        public string SahaAdi { get; set; } = string.Empty;

        [Required]
        public string SahaKodu { get; set; } = string.Empty; // URL'de kullanılacak: "antalya-arena"

        [Range(0, 100000)]
        public decimal SaatlikUcret { get; set; } = 3000;
    }
}
