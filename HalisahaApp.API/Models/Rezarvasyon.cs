namespace HalisahaApp.API.Models
{
    public class Rezervasyon
    {
        public int Id { get; set; }

        // Saha ilişkisi
        public int SahaId { get; set; }
        public Saha Saha { get; set; } = null!;

        // Rezervasyon yapan kişi (kayıt yok, sadece isim)
        public string AdSoyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;

        // Tarih & Saat
        public DateTime Tarih { get; set; }          // Hangi gün
        public int BaslangicSaati { get; set; }      // Örn: 19 → 19:00
        public int BitisSaati { get; set; }          // Örn: 20 → 20:00

        // Ödeme
        public decimal ToplamUcret { get; set; }
        public OdemeDurumu OdemeDurumu { get; set; } = OdemeDurumu.Bekliyor;
        public string? IyzicoOdemeId { get; set; }   // iyzico'dan dönen ID

        // Durum
        public RezervasyonDurumu Durum { get; set; } = RezervasyonDurumu.Aktif;

        // Tarihler
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public DateTime? IptalTarihi { get; set; }
        public string? IptalNedeni { get; set; }

        // Hesaplanan özellikler
        public string SaatAraligi => $"{BaslangicSaati:D2}:00 – {BitisSaati:D2}:00";
        public string TarihVeSaat => $"{Tarih:dd MMMM yyyy} {SaatAraligi}";
    }

    public enum OdemeDurumu
    {
        Bekliyor = 0,
        Odendi = 1,
        IadaEdildi = 2,
        Basarisiz = 3
    }

    public enum RezervasyonDurumu
    {
        Aktif = 0,
        Iptal = 1,
        Tamamlandi = 2
    }
}
